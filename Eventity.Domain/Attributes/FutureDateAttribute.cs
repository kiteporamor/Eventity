using System;
using System.ComponentModel.DataAnnotations;

namespace Eventity.Domain.Attributes;

public class FutureDateAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is DateTime dateTime)
        {
            return dateTime > DateTime.UtcNow;
        }
        return false;
    }
}
