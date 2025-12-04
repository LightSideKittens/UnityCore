using System;
using System.Collections.Generic;
using Sendbird.Chat;
using Sirenix.OdinInspector;
using UnityEngine;

public class ClubTest : MonoBehaviour
{
    [Serializable]
    public class Club
    {
        public SbGroupChannel channel;
        public string url;
        public string name;
        public int maxMembers;
        [ProgressBar(0, "maxMembers")]
        public int members;

        public void Setup(SbGroupChannel channel)
        {
            this.channel = channel;
            url = channel.Url;
            name = channel.Name;
            maxMembers = 30;
            members = channel.Members.Count;
        }

        [Button]
        public void Join()
        {
            BlaBla.Club.Join(channel, () =>
            {
                Debug.Log("Joined");
            });
        }
        
        [Button]
        public void Leave()
        {
            BlaBla.Club.Leave(channel, () =>
            {
                Debug.Log("Leaved");
            });
        }
    }
    
    [ShowInInspector] public SbUser user;
    [ShowInInspector] public SbGroupChannelCreateParams createParams;
    public List<Club> channels = new();

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
    public void GetClubLoader()
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
            for (int i = 0; i < x.Count; i++)
            {
                var club = new Club();
                club.Setup(x[i]);
                channels.Add(club);
            }
        });
    }
}