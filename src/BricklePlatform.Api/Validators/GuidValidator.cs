using System;

namespace BricklePlatform.Api.Validators
{
    public class GuidValidator
    {
        public static bool IsGuid(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return false;
            }

            if (!Guid.TryParse(guid, out _))
            {
                return false;
            }

            return true;
        }
    }
}