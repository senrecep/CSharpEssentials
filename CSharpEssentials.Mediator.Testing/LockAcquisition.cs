namespace CSharpEssentials.Locking.Testing;

/// <summary>One call recorded by <see cref="RecordingResourceLock"/>.</summary>
/// <param name="Key">The lock key.</param>
/// <param name="Timeout">The timeout of <c>AcquireAsync</c>; <see langword="null"/> for no timeout or for <c>TryAcquireAsync</c>.</param>
/// <param name="TryOnly"><see langword="true"/> for <c>TryAcquireAsync</c>.</param>
public sealed record LockAcquisition(string Key, TimeSpan? Timeout, bool TryOnly);
