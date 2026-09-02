// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._BRatbite.TrackingHud;
using Content.Shared._BRatbite.TrackingHud.MarkerMonitor;
using Content.Shared.Radio;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.PanicButton
{
    [RegisterComponent]
    public sealed partial class PanicButtonComponent : Component
    {
        /// <summary>
        /// What message to send over the radio.
        /// </summary>
        [DataField]
        public LocId DistressMessage = "panic-button-distress";

        /// <summary>
        /// How long is the cooldown before you can send another message.
        /// </summary>
        [DataField]
        public TimeSpan CoolDown = TimeSpan.FromSeconds(70);

        /// <summary>
        /// Which channel to send the message over.
        /// </summary>
        [DataField]
        public ProtoId<RadioChannelPrototype> RadioChannel = "Security";

        // Ratbite start
        [DataField]
        public ProtoId<MarkerPrototype> MarkerPrototype = "WarningExclamation";

        [DataField]
        public Color SecHudIconColor = Color.Red;

        [DataField]
        public ListeningChannels Channels = ListeningChannels.SECURITY;

        [DataField]
        public SoundSpecifier PlayedSound = new SoundPathSpecifier("/Audio/_BRatbite/SecHud/siren.ogg");
        // Ratbite end
    }
}
