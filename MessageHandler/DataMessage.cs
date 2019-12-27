public class DataMessage<T> : Message 
{ 
    public DataMessage(string messageName, T data) : base(messageName)
    {
        this.data = data;
        this.dataType = data.GetType();
    }

    public T data;
}