﻿namespace PaxAndromeda.Instar;

public static class Utilities
{
    public static List<T>? GetAttributesOfType<T>(this Enum enumVal) where T : Attribute
    {
        var type = enumVal.GetType();
        var membersInfo = type.GetMember(enumVal.ToString());
        if (membersInfo.Length == 0)
            return null;

        var attributes = membersInfo[0].GetCustomAttributes(typeof(T), false);
        return attributes.Length > 0 ? attributes.OfType<T>().ToList() : null;
    }
}