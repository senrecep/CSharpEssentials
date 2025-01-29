using Google.Api.Gax.ResourceNames;
using Google.Cloud.SecretManager.V1;

namespace CSharpEssentials.GcpSecretManager.Models.Internal;

internal sealed record ProjectSecretLoadContext(
    SecretManagerServiceClient Client,
    ProjectName ProjectName,
    ProjectSecretConfiguration Config);