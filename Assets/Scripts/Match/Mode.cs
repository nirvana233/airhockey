using System;

namespace AirHockey.Match
{
    public enum Mode
    {
        HighScore,
        BestOfScore,
        Time,
        Endless
    }

    public static class ModeUtils
    {
        public static string InfoName(this Mode mode)
        {
            switch (mode)
            {
                case Mode.HighScore:
                    return "score";
                case Mode.BestOfScore:
                    return "scores";
                case Mode.Time:
                    return "duration";
                case Mode.Endless:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "Endless mode doesn't require info.");
                default:
                    throw new NotImplementedException($"Mode not implemented: {mode}.");
            }
        }
        
        public static string InfoUnitName(this Mode mode)
        {
            switch (mode)
            {
                case Mode.HighScore:
                case Mode.BestOfScore:
                    return "points";
                case Mode.Time:
                    return "minutes";
                case Mode.Endless:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "Endless mode doesn't require info.");
                default:
                    throw new NotImplementedException($"Mode not implemented: {mode}.");
            }
        }
    }
}