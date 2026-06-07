// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Popups;

namespace Content.Ratbite.Shared.Abilities;

public abstract partial class SharedCrawlUnderObjectsSystem : EntitySystem
{
    [Dependency] private SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CrawlUnderObjectsComponent, CrawlingUpdatedEvent>(OnCrawlingUpdated);
    }

    private void OnCrawlingUpdated(EntityUid uid,
        CrawlUnderObjectsComponent component,
        CrawlingUpdatedEvent args)
    {
        if (args.Enabled)
            _popup.PopupEntity(Loc.GetString("crawl-under-objects-toggle-on"), uid);
        else
            _popup.PopupEntity(Loc.GetString("crawl-under-objects-toggle-off"), uid);
    }
}
