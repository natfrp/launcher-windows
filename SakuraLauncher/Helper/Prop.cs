using SakuraLauncher.Model;

namespace SakuraLauncher.Helper
{
    /// <summary>
    /// Deprecated, don't use.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Prop<T> : ModelBase
    {
        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                RaisePropertyChanged();
            }
        }
        private T _value;

        public Prop(T initial = default(T)) => _value = initial;

        public override string ToString() => _value.ToString();

        public static implicit operator T(Prop<T> p) => p.Value;
        public static explicit operator Prop<T>(T initial) => new Prop<T>(initial);
    }
}
