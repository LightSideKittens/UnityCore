using System.Collections.Generic;
using Sendbird.Chat;
using Sirenix.OdinInspector;
using UnityEngine;

public class ClubTest : MonoBehaviour
{
    [ShowInInspector] public SbUser user;
    [ShowInInspector] public SbGroupChannelCreateParams createParams;
    public List<SbGroupChannel> channels = new();

    [Button]
    public void Connect()
    {
        BlaBla.Connect(x =>
        {
            user = x;
        });
    }

    [Button]
    public void CreateGroup()
    {
        createParams = new SbGroupChannelCreateParams();
        BlaBla.Club.Create(createParams, channel =>
        {
            Debug.Log(channel.Url);
        });
    }

    private SbPublicGroupChannelListQuery loader;

    [Button]
    public void GetChannels()
    {
        var query = BlaBla.Club.Ref.Limit(50);
        BlaBla.Club.GetPageLoader(query, loader =>
        {
            this.loader = loader;
        });
    }
    
    private bool CanLoadNext => loader is { HasNext: true };

    [Button]
    [ShowIf("CanLoadNext")]
    public void LoadNext()
    {
        loader.LoadNextPage((x, error) =>
        {
            channels.AddRange(x);
        });
    }
}