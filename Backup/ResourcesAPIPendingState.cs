using System;
using System.Runtime.CompilerServices;

internal class ResourcesAPIPendingState
{
    public ResourcesAPI.ResourceLookupCallback Callback { get; set; }

    public object UserContext { get; set; }
}

