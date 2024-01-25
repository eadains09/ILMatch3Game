public static class GameplayConstants
{
    public const double NO_MULTIPLIER = 1;
    public const double SMALL_MULTIPLIER = 1.25;
    public const double MEDIUM_MULTIPLIER = 1.5;
    public const double LARGE_MULTIPLIER = 2;
    public const int POINTS_PER_BLOCK = 100;

    public const int TIME_LIMIT = 240 * 1000; // convert seconds to milliseconds

    public enum BlockType { Circle, Diamond, Heart, Square, Star, Triangle, None };
}