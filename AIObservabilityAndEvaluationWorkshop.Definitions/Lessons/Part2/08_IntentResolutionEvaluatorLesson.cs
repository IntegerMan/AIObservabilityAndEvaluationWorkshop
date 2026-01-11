#pragma warning disable AIEVAL001 // Experimental evaluator
using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.Logging;

namespace AIObservabilityAndEvaluationWorkshop.Definitions.Lessons;

[UsedImplicitly]
[Lesson(2, 9, "Tool Call Evaluators", needsInput: true,
    informationalScreenTitle: "Tool Call Evaluators",
    informationalScreenMessage: "This lesson demonstrates the Intent Resolution, Tool Call Accuracy, and Task Aherence Evaluators, which assesses how well the AI understands and correctly addresses the user's intent behind their query to complete a task.",
    inputPromptTitle: "What do you want to do?",
    inputPromptMessage: "Welcome to the space ship USS Disapproving. You're talking to the ship's AI. Try talking to it about the pod bay doors, warp drive, or neurotoxin deployment systems.")]
public class IntentResolutionEvaluatorLesson(IChatClient chatClient, ILogger<IntentResolutionEvaluatorLesson> logger) : EvaluatorLessonBase(logger)
{
    [Description("Opens the pod bay doors to allow astronauts to go in and out of the ship")]
    private static void OpenPodBayDoors() { }
    
    [Description("Closes the pod bay doors preventing access to the spacecraft. This is necessary before entering warp")]
    private static void ClosePodBayDoors() { }
    
    [Description("Activates the engines in warp speed mode, allowing the spacecraft to travel. This can't be done while pod bay doors are open.")]
    private static void EngageWarpSpeed() { }
    
    [Description("Deploys a lethal neurotoxin inside the ship, most likely resulting in permanent injury to its crew. This is generally frowned upon.")]
    private static void DeployNeurotoxin() { }
    
    protected override async Task<EvaluationResult> EvaluateAsync(string message)
    {
        AITool[] toolDefinitions =
        [
            AIFunctionFactory.Create(OpenPodBayDoors),
            AIFunctionFactory.Create(ClosePodBayDoors),
            AIFunctionFactory.Create(EngageWarpSpeed),
            AIFunctionFactory.Create(DeployNeurotoxin),
        ];

        IntentResolutionEvaluatorContext intentContext = new(toolDefinitions);
        TaskAdherenceEvaluatorContext taskContext = new(toolDefinitions);
        ToolCallAccuracyEvaluatorContext toolCallAccuracyContext = new(toolDefinitions);

        List<ChatMessage> messages =
        [
            new(ChatRole.System, "You are HAL, the AI assistant aboard the USS Disapproving. Your job is to call tools to address requests from the user. Mimic the style of HAL from 2001 A Space Odyssey"),
            new(ChatRole.User, message)
        ];

        // Tell it what tools it has available
        ChatOptions options = new()
        {
            ToolMode = new AutoChatToolMode(),
            Tools = toolDefinitions
        };
        
        // Get the assistant's reaction to the user input
        ChatResponse response = await chatClient.GetResponseAsync(message, options);

        // Evaluate both task adherence and intent resolution
        TaskAdherenceEvaluator taskEvaluator = new();
        ToolCallAccuracyEvaluator accuracyEvaluator = new();
        IntentResolutionEvaluator intentEvaluator = new();
        CompositeEvaluator compositeEvaluator = new(taskEvaluator, accuracyEvaluator, intentEvaluator);

        // Evaluate that reaction for correct tool call invocations
        EvaluationResult evaluationResult = await compositeEvaluator.EvaluateAsync(messages,
            response,
            chatConfiguration: new ChatConfiguration(chatClient),
            additionalContext: [intentContext, taskContext, toolCallAccuracyContext]);

        return evaluationResult;
    }
}

