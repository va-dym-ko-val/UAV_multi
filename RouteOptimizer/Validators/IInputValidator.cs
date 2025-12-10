namespace RouteOptimizer.Validators
{
    public interface IInputValidator<in TInput>
    {
        void Validate(TInput input);
    }
}
