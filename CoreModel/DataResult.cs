namespace CoreModel
{
    public class DataResult<T>
    {
        public MainResult MainResult { get; set; }
        public T Data { get; set; }
    }
}
