using TaskGenerator.Models;

namespace TaskGenerator
{
    public interface ITaskGenerator<in TInput, out TOutput> 
        where TInput : TaskInputData 
        where TOutput : TaskOutputData, new()
    {
        TOutput GenerateTaskData(TInput inputData);
    }
}