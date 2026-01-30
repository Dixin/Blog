namespace Examples.Threading;

//Base instance = new Derived();
//instance.Method();

file class Base
{
    public virtual void Method() { }
}

file class Derived : Base
{
    public override async void Method()
    {
        base.Method();
        await Task.Yield();
    }
}