namespace EasyContainer
{
    public interface IQux
    {

    }

    [MapTo(typeof(IQux), Lifetime.Root)]
    public class Qux : Base, IQux
    {
    }
}
