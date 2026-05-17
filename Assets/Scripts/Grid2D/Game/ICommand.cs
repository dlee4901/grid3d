public interface ICommand
{
    bool ApplyTo(GridState state);
}
