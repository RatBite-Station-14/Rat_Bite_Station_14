// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Audio.Headphones;
using Content.Shared.Audio.Jukebox;
using Robust.Client.Audio;
using Robust.Client.UserInterface;
using Robust.Shared.Audio.Components;
using Robust.Shared.Prototypes;

namespace Content.Client.Audio.Headphones;

public sealed class HeadphoneMusicBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private HeadphoneMusicMenu? _menu;

    public HeadphoneMusicBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<HeadphoneMusicMenu>();
        _menu.OnSongSelected += SelectSong;
        _menu.SetTime += SetTime;

        PopulateMusic();
        Reload();
    }

    public void Reload()
    {
        if (_menu == null || !EntMan.TryGetComponent(Owner, out HeadphoneMusicComponent? headphones))
            return;

        _menu.SetAudioStream(headphones.AudioStream);

        if (_prototype.TryIndex(headphones.SelectedSongId, out var song))
        {
            var length = EntMan.System<AudioSystem>().GetAudioLength(song.Path.Path.ToString());
            _menu.SetSelectedSong(song.Name, (float) length.TotalSeconds);
        }
        else
        {
            _menu.SetSelectedSong(string.Empty, 0f);
        }
    }

    private void PopulateMusic()
    {
        _menu?.Populate(_prototype.EnumeratePrototypes<JukeboxPrototype>());
    }

    private void SelectSong(ProtoId<JukeboxPrototype> songId)
    {
        SendMessage(new HeadphoneMusicSelectedMessage(songId));
    }

    private void SetTime(float time)
    {
        if (EntMan.TryGetComponent(Owner, out HeadphoneMusicComponent? headphones) &&
            EntMan.TryGetComponent(headphones.AudioStream, out AudioComponent? audio))
        {
            audio.PlaybackPosition = time;
        }

        SendMessage(new HeadphoneMusicSetTimeMessage(time));
    }
}
