using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SpaceWarp.Backend.Modding;

public class Ksp2ModInfo
{
    [JsonConverter(typeof(VersionConverter))]
    [JsonProperty]
    public Version APIVersion { get; private set; }

    // Token: 0x17001C93 RID: 7315
    // (get) Token: 0x06008070 RID: 32880 RVA: 0x001EE0B9 File Offset: 0x001EC2B9
    // (set) Token: 0x06008071 RID: 32881 RVA: 0x001EE0C1 File Offset: 0x001EC2C1
    [JsonConverter(typeof(VersionConverter))]
    [JsonProperty]
    public Version ModVersion { get; private set; }

    // Token: 0x17001C94 RID: 7316
    // (get) Token: 0x06008072 RID: 32882 RVA: 0x001EE0CA File Offset: 0x001EC2CA
    // (set) Token: 0x06008073 RID: 32883 RVA: 0x001EE0D2 File Offset: 0x001EC2D2
    [JsonProperty]
    public string ModName { get; private set; }

    // Token: 0x17001C95 RID: 7317
    // (get) Token: 0x06008074 RID: 32884 RVA: 0x001EE0DB File Offset: 0x001EC2DB
    // (set) Token: 0x06008075 RID: 32885 RVA: 0x001EE0E3 File Offset: 0x001EC2E3
    [JsonProperty]
    public string ModAuthor { get; private set; }

    // Token: 0x17001C96 RID: 7318
    // (get) Token: 0x06008076 RID: 32886 RVA: 0x001EE0EC File Offset: 0x001EC2EC
    // (set) Token: 0x06008077 RID: 32887 RVA: 0x001EE0F4 File Offset: 0x001EC2F4
    [JsonProperty]
    public string ModDescription { get; private set; }

    // Token: 0x17001C97 RID: 7319
    // (get) Token: 0x06008078 RID: 32888 RVA: 0x001EE0FD File Offset: 0x001EC2FD
    // (set) Token: 0x06008079 RID: 32889 RVA: 0x001EE105 File Offset: 0x001EC305
    [JsonProperty]
    public string Catalog { get; private set; }
}