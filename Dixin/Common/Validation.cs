namespace Dixin.Common
{
    public class Validation<T>
    {
        internal Validation(T value, string name)
        {
            this.Value = value;
            this.Name = name;
        }

        public T Value { get; }

        public string Name { get; }

        public void IsNotNull() => this.Value.IsNotNull(this.Name);
    }
}