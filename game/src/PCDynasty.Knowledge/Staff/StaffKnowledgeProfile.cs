using System.Collections.Immutable;

namespace PCDynasty.Knowledge.Staff;

public sealed class StaffKnowledgeProfile
{
    public StaffKnowledgeProfile(
        string staffId,
        IEnumerable<KnowledgeCapability> capabilities)
    {
        StaffId = StaffKnowledgeValue.Required(staffId, nameof(staffId));
        ArgumentNullException.ThrowIfNull(capabilities);
        var keys = new HashSet<(string DomainId, string RegionId)>();
        Capabilities = capabilities.Select(capability =>
        {
            ArgumentNullException.ThrowIfNull(capability);
            if (!keys.Add((capability.Domain.DomainId, capability.Domain.RegionId)))
            {
                throw new ArgumentException(
                    "Knowledge domain and region pairs must be unique.",
                    nameof(capabilities));
            }

            return capability;
        }).OrderBy(capability => capability.Domain.RegionId, StringComparer.Ordinal)
          .ThenBy(capability => capability.Domain.DomainId, StringComparer.Ordinal)
          .ToImmutableArray();
    }

    public string StaffId { get; }
    public ImmutableArray<KnowledgeCapability> Capabilities { get; }

    public KnowledgeCapability Get(string domainId, string regionId)
    {
        string normalizedDomain = StaffKnowledgeValue.Required(domainId, nameof(domainId));
        string normalizedRegion = StaffKnowledgeValue.Required(regionId, nameof(regionId));
        return Capabilities.SingleOrDefault(capability =>
            string.Equals(capability.Domain.DomainId, normalizedDomain, StringComparison.Ordinal) &&
            string.Equals(capability.Domain.RegionId, normalizedRegion, StringComparison.Ordinal)) ??
            throw new KeyNotFoundException(
                $"No knowledge capability exists for '{normalizedDomain}' in '{normalizedRegion}'.");
    }
}
