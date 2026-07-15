public enum StatusType
{
    Vulnerable
}

public class StatusInstance
{
    public StatusType Type;
    public int Stacks;
    
    public StatusInstance(StatusType type, int stacks)
    {
        Type = type;
        Stacks = stacks;
    }
}