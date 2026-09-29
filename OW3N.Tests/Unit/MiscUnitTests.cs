using Xunit;

namespace OW3N.Tests.Unit;

/// <summary>Small focused tests for <see cref="Gauge"/> and <see cref="LiveUpdateService"/>.</summary>
public class GaugeTests
{
    [Fact]
    public void GaugeTypeLookup_maps_known_gauges_to_render_types()
    {
        Assert.Equal(GaugeType.IconBar, Gauge.GaugeTypeLookup["health"]);
        Assert.Equal(GaugeType.Orb, Gauge.GaugeTypeLookup["mana"]);
        Assert.Equal(GaugeType.IconBar, Gauge.GaugeTypeLookup["hunger"]);
    }
}

public class LiveUpdateServiceTests
{
    [Fact]
    public void NotifyCharacterChanged_invokes_subscribers_with_id()
    {
        var service = new LiveUpdateService();
        int? received = null;
        service.OnCharacterChanged += id => received = id;

        service.NotifyCharacterChanged(42);

        Assert.Equal(42, received);
    }

    [Fact]
    public void NotifyCampaignChanged_invokes_subscribers_with_id()
    {
        var service = new LiveUpdateService();
        int? received = null;
        service.OnCampaignChanged += id => received = id;

        service.NotifyCampaignChanged(7);

        Assert.Equal(7, received);
    }

    [Fact]
    public void Notify_does_not_throw_without_subscribers()
    {
        var service = new LiveUpdateService();
        service.NotifyCharacterChanged(1);
        service.NotifyCampaignChanged(1);
    }
}
