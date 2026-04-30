using System.Collections.Generic;
// Data class, contains a bunch of BoardStates in sequence

public class Timeline
{
    public int TimelineId;
    public int CreatedOnTurn;

    private readonly List<BoardState> boardStates = new();

    // Timelines each have an ID (Held by the GameManager), and a CreatedOnTurn
    public Timeline(int timelineId, int createdOnTurn)
    {
        TimelineId = timelineId;
        CreatedOnTurn = createdOnTurn;
    }

    public void AddState(BoardState state)
    {
        boardStates.Add(state);
    }

    // Returns the rightmost state
    public BoardState GetLatestState()
    {
        if (boardStates.Count == 0) return null;

        return boardStates[^1];
    }

    // Pretty self explanitory
    public BoardState GetStateAtTurn(int turn)
    {
        int index = turn - CreatedOnTurn;
        if (index < 0 || index >= boardStates.Count) return null;
        
        return boardStates[index];
    }
}