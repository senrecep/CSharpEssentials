#!/usr/bin/env python3
"""Generates tests/vectors/idempotency-key-vectors.json, this repo's deterministic key, scope and storage key vectors.
Standard library only, so the expected values come from an implementation independent of the library under test.

Usage: python3 scripts/generate-idempotency-key-vectors.py
"""

import base64
import hashlib
import json
import pathlib
import uuid

NAMESPACE = uuid.UUID("9c8fbd17-da5f-4f32-a4bf-651721368d5e")
MAX_KEY_LENGTH = 255
MESSAGE_OPERATION = "message"
STORAGE_PREFIX = "idem:v3:"
GLOBAL_SCOPE = "global"


def is_allowed_char(ch: str) -> bool:
    code = ord(ch)
    return 0x21 <= code <= 0x7E and ch not in ('"', "\\")


def is_valid_key(value: str) -> bool:
    return all(is_allowed_char(ch) for ch in value) and 1 <= len(value) <= MAX_KEY_LENGTH


def canonical(operation: str, ids: list[str]) -> str:
    parts = [operation, *ids]
    return "|".join(f"{len(part.encode('utf-8'))}:{part}" for part in parts)


def key_for(operation: str, ids: list[str]) -> str:
    text = canonical(operation, ids)
    if is_valid_key(text):
        return text
    digest = hashlib.sha256(text.encode("utf-8")).digest()
    return "h:" + base64.urlsafe_b64encode(digest).rstrip(b"=").decode("ascii")


def uuid_v5(operation: str, ids: list[str]) -> str:
    return str(uuid.uuid5(NAMESPACE, canonical(operation, ids)))


def length_prefixed(part: str) -> str:
    return f"{len(part.encode('utf-8'))}:{part}"


def present(value):
    return value is not None and value != ""


def scope_of(kind: str, value):
    return f"{kind}:{value}" if present(value) else None


def scope_user_and_tenant(tenant_id, user_id):
    if not present(tenant_id) or not present(user_id):
        return None
    tenant = f"tenant:{tenant_id}"
    return f"{length_prefixed(tenant)}/user:{user_id}"


def scope_vector(name: str, kind: str, **inputs) -> dict:
    if kind == "global":
        scope = GLOBAL_SCOPE
    elif kind == "userAndTenant":
        scope = scope_user_and_tenant(inputs["tenantId"], inputs["userId"])
    elif kind == "custom":
        scope = scope_of(inputs["customKind"], inputs["value"])
    else:
        scope = scope_of(kind, inputs["id"])
    return {"name": name, "kind": kind, **inputs, "scope": scope}


def storage_key(prefix: str, namespace: str, scope: str, key: str) -> str:
    return prefix + "|".join(length_prefixed(part) for part in (namespace, scope, key))


def storage_vector(name: str, namespace: str, scope: str, key: str) -> dict:
    return {
        "name": name,
        "prefix": STORAGE_PREFIX,
        "namespace": namespace,
        "scope": scope,
        "key": key,
        "storageKey": storage_key(STORAGE_PREFIX, namespace, scope, key),
    }


def for_vector(name: str, operation: str, ids: list[str]) -> dict:
    return {
        "name": name,
        "operation": operation,
        "ids": ids,
        "canonical": canonical(operation, ids),
        "key": key_for(operation, ids),
        "uuidV5": uuid_v5(operation, ids),
    }


def message_vector(name: str, message_id: str, consumer: str) -> dict:
    ids = [consumer, message_id]
    return {
        "name": name,
        "messageId": message_id,
        "consumer": consumer,
        "canonical": canonical(MESSAGE_OPERATION, ids),
        "key": key_for(MESSAGE_OPERATION, ids),
    }


def main() -> None:
    padding_255 = "x" * (MAX_KEY_LENGTH - len("7:capture|") - len("246:"))
    padding_256 = padding_255 + "x"
    padding_long = "x" * 1000
    assert len(canonical("capture", [padding_255])) == 255
    assert len(canonical("capture", [padding_256])) == 256

    vectors_for = [
        for_vector("single id", "capture", ["order-42"]),
        for_vector("operation only", "capture", []),
        for_vector("boundary a|bc", "op", ["a", "bc"]),
        for_vector("boundary ab|c", "op", ["ab", "c"]),
        for_vector("separators inside parts", "op", ["a|b", "1:2"]),
        for_vector("empty id", "op", [""]),
        for_vector("two empty ids", "op", ["", ""]),
        for_vector("several ids", "refund", ["tenant-1", "order-42", "line-7"]),
        for_vector("exactly 255 characters", "capture", [padding_255]),
        for_vector("256 characters falls back to hash", "capture", [padding_256]),
        for_vector("1000-character id falls back to hash", "capture", [padding_long]),
        for_vector("space falls back to hash", "op", ["a b"]),
        for_vector("double quote falls back to hash", "op", ['a"b']),
        for_vector("backslash falls back to hash", "op", ["a\\b"]),
        for_vector("turkish characters fall back to hash", "ödeme", ["şirket-ığü"]),
        for_vector("emoji falls back to hash", "op", ["🎉"]),
        for_vector("uuid id", "capture", ["0192f3b4-7c1e-7a00-8000-000000000001"]),
    ]

    vectors_message = [
        message_vector("message", "msg-1", "billing"),
        message_vector("message swapped", "billing", "msg-1"),
        message_vector("message other consumer", "msg-1", "shipping"),
        message_vector("message guid id", "0192f3b4-7c1e-7a00-8000-000000000001", "OrderPlacedConsumer"),
    ]

    vectors_scope = [
        scope_vector("user", "user", id="u1"),
        scope_vector("tenant", "tenant", id="t1"),
        scope_vector("client", "client", id="billing-service"),
        scope_vector("custom", "custom", customKind="account", value="acc-9"),
        scope_vector("global", "global"),
        scope_vector("user and tenant", "userAndTenant", tenantId="t1", userId="u1"),
        scope_vector("user and tenant with separators", "userAndTenant", tenantId="a/b:c", userId="d|e"),
        scope_vector("user and tenant unicode", "userAndTenant", tenantId="şirket", userId="kullanıcı-ğ"),
        scope_vector("user unicode", "user", id="müşteri-ı"),
        scope_vector("user emoji", "user", id="🎉"),
        scope_vector("user missing", "user", id=None),
        scope_vector("user empty", "user", id=""),
        scope_vector("tenant missing", "tenant", id=None),
        scope_vector("client empty", "client", id=""),
        scope_vector("custom value missing", "custom", customKind="account", value=None),
        scope_vector("user and tenant without tenant", "userAndTenant", tenantId=None, userId="u1"),
        scope_vector("user and tenant without user", "userAndTenant", tenantId="t1", userId=None),
        scope_vector("user and tenant with empty tenant", "userAndTenant", tenantId="", userId="u1"),
    ]

    vectors_storage = [
        storage_vector("tenant scope", "capture", "tenant:t1", "abc"),
        storage_vector("http user scope", "http", "user:u1", "order-42"),
        storage_vector("global scope", "http", GLOBAL_SCOPE, "abc"),
        storage_vector("user and tenant scope", "http", "9:tenant:t1/user:u1", "abc"),
        storage_vector("unicode namespace and scope", "SiparişOluştur", "user:müşteri", "abc"),
        storage_vector("separators inside parts", "a|b", "user:1:2", "x|y"),
        storage_vector("hashed deterministic key", "message", "client:billing", key_for("capture", ["x" * 300])),
    ]

    document = {
        "version": 2,
        "spec": {
            "parts": "For(operation, ...ids): parts are [operation, ...ids] in order.",
            "canonical": "Each part is written as '{utf8ByteLength}:{value}' (decimal length, no padding) and the parts are joined with '|'.",
            "allowedCharacters": "A key character is in U+0021..U+007E except '\"' (U+0022) and '\\\\' (U+005C).",
            "key": "If every character of the canonical string is allowed and its length is 1..255, the key is the canonical string. Otherwise it is 'h:' + base64url(SHA-256(UTF-8(canonical))) without padding. The character check comes first, so the length is always counted over ASCII characters.",
            "fromMessage": "FromMessage(messageId, consumer) = For('message', consumer, messageId).",
            "uuidV5": "ToUuidV5(operation, ...ids) = RFC 9562 UUIDv5 of UTF-8(canonical) in namespaceUuid, lowercase with hyphens. The canonical string is hashed whether or not the key falls back to 'h:'.",
            "invalidInput": "operation null or empty, any id null, or any part that is not well-formed UTF-16 (lone surrogate) throws an argument error in each language. Lone surrogates cannot round-trip through every JSON parser, so each language tests them natively; the 'invalid' list holds the portable cases.",
            "scopes": "user, tenant and client scopes are '{kind}:{id}'; custom is '{customKind}:{value}'; global is the literal 'global'. userAndTenant builds tenant = 'tenant:' + tenantId and user = 'user:' + userId, then the scope is '{utf8ByteLength(tenant)}:{tenant}/{user}' (only the tenant part is length-prefixed). A null or empty id, value, tenantId or userId means no scope (null in the vectors).",
            "storageKeys": "storageKey = prefix + [namespace, scope, key] each written as '{utf8ByteLength}:{value}' and joined with '|'. The default prefix is 'idem:v3:'. A global key uses the scope 'global'.",
            "validation": "Servers accept an Idempotency-Key header with the same allowed characters and length 1..MaxKeyLength (default 255).",
        },
        "namespaceUuid": str(NAMESPACE),
        "maxKeyLength": MAX_KEY_LENGTH,
        "messageOperation": MESSAGE_OPERATION,
        "rfc9562": {
            "namespace": str(uuid.NAMESPACE_DNS),
            "name": "www.example.com",
            "uuidV5": str(uuid.uuid5(uuid.NAMESPACE_DNS, "www.example.com")),
        },
        "storagePrefix": STORAGE_PREFIX,
        "for": vectors_for,
        "fromMessage": vectors_message,
        "scopes": vectors_scope,
        "storageKeys": vectors_storage,
        "invalid": [
            {"name": "empty operation", "operation": "", "ids": ["a"]},
            {"name": "null operation", "operation": None, "ids": ["a"]},
            {"name": "null id", "operation": "op", "ids": ["a", None]},
        ],
    }

    assert document["rfc9562"]["uuidV5"] == "2ed6657d-e927-568b-95e1-2665a8aea6a2"
    assert vectors_for[2]["key"] != vectors_for[3]["key"]
    assert vectors_message[0]["key"] != vectors_message[1]["key"]
    assert vectors_scope[5]["scope"] == "9:tenant:t1/user:u1"
    assert vectors_storage[0]["storageKey"] == "idem:v3:7:capture|9:tenant:t1|3:abc"

    path = pathlib.Path(__file__).resolve().parent.parent / "tests" / "vectors" / "idempotency-key-vectors.json"
    path.write_text(json.dumps(document, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(path)


if __name__ == "__main__":
    main()
