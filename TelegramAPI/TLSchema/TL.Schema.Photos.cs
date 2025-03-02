using System.Collections.Generic;

namespace TL
{
#pragma warning disable CS1574
    /// <summary>Full list of photos with auxiliary data.		<para>See <a href="https://corefork.telegram.org/constructor/photos.photos"/></para></summary>
    [TLDef(0x8DCA6AA5)]
    public partial class Photos_Photos : IObject
    {
        /// <summary>List of photos</summary>
        public PhotoBase[] photos;
        /// <summary>List of mentioned users</summary>
        public Dictionary<long, User> users;
    }
    
    /// <summary>Incomplete list of photos with auxiliary data.		<para>See <a href="https://corefork.telegram.org/constructor/photos.photosSlice"/></para></summary>
    [TLDef(0x15051F54)]
    public sealed partial class Photos_PhotosSlice : Photos_Photos
    {
        /// <summary>Total number of photos</summary>
        public int count;
    }

    /// <summary>Photo with auxiliary data.		<para>See <a href="https://corefork.telegram.org/constructor/photos.photo"/></para></summary>
    [TLDef(0x20212CA8)]
    public sealed partial class Photos_Photo : IObject
    {
        /// <summary>Photo</summary>
        public PhotoBase photo;
        /// <summary>Users</summary>
        public Dictionary<long, User> users;
    }
}