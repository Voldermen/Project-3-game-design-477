using System.Collections.Generic;

public class Timeline
{
    public int TimelineId;
    public int CreatedOnTurn;

    private readonly List<BoardState> boardStates = new();

    public Timeline(int timelineId, int createdOnTurn)
    {
        TimelineId = timelineId;
        CreatedOnTurn = createdOnTurn;
    }

    public void AddState(BoardState state)
    {
        boardStates.Add(state);
    }

    public BoardState GetLatestState()
    {
        if (boardStates.Count == 0)
        {
            return null;
        }

        return boardStates[^1];
    }

    public BoardState GetStateAtTurn(int turn)
    {
        int index = turn - CreatedOnTurn;

        if (index < 0 || index >= boardStates.Count)
        {
            return null;
        }

        return boardStates[index];
    }
}