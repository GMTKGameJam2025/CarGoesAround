public struct GameOverEvent : IGameEvent
{
    public string GameOverMessage;

    public GameOverEvent(string message = "You suck")
    {
        GameOverMessage = message;
    }
}