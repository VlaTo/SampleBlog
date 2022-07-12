using Microsoft.AspNetCore.Components;
using SampleBlog.Core.Domain.Entities;
using Outcome = SampleBlog.Web.Shared.Models.Menu.Outcome;

namespace SampleBlog.Web.Client.Core.Extensions;

internal static class OutcomeExtensions
{
    public static MarkupString ToString(this Outcome outcome, IFormatProvider formatProvider)
    {
        switch (outcome.Units)
        {
            case Units.Kilo:
            {
                return new MarkupString(String.Format(formatProvider, "{0:N2} кг.", outcome.Amount));
            }

            case Units.Grams:
            {
                return new MarkupString(String.Format(formatProvider, "{0:N0} гр.", outcome.Amount));
            }

            case Units.Pieces:
            {
                return new MarkupString(String.Format(formatProvider, "{0:N0} кус.", outcome.Amount));
            }

            case Units.Portion:
            {
                return new MarkupString(String.Format(formatProvider, "{0:N0} шт.", outcome.Amount));
            }
        }

        return new MarkupString();
    }
}