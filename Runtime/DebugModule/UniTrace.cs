using System.Diagnostics;
using System.IO;
using UnityEngine;

public class UniTrace
{
    private const int BaseOffset = 1;
    private int frameOffset;
    private readonly StackTrace stackTrace;
    private StackFrame currentFrame;

    private UniTrace(StackTrace stackTrace, int frameOffset)
    {
        this.stackTrace = stackTrace;
        this.frameOffset = frameOffset;
        currentFrame = stackTrace.GetFrame(frameOffset);
    }

    public static implicit operator string(UniTrace trace)
    {
        trace.FilePath(out var path)
            .MethodName(out var method)
            .Line(out var line);
        
        return $"{path}\n: {method}\n: {line}";
    }

    public static UniTrace Create(int frameOffset = 0)
    {
        return new UniTrace(new StackTrace(BaseOffset, true), frameOffset);
    }

    public UniTrace NextFrame()
    {
        currentFrame = stackTrace.GetFrame(frameOffset++);
        return this;
    }
    
    public UniTrace PreviousFrame()
    {
        currentFrame = stackTrace.GetFrame(frameOffset--);
        return this;
    }

    public UniTrace FilePath(out string filePath)
    {
        filePath = currentFrame.GetFileName()?[Application.dataPath.Length..];
        return this;
    }
    
    public UniTrace FileName(out string fileName)
    {
        FilePath(out var filePath);
        fileName = Path.GetFileName(filePath);
        return this;
    }
    
    public UniTrace ClassName(out string className)
    {
        className = currentFrame.GetMethod()?.DeclaringType?.Name;
        return this;
    }
    
    public UniTrace MethodName(out string methodName)
    {
        methodName = currentFrame.GetMethod().Name;
        return this;
    }
    
    public UniTrace Line(out int line)
    {
        line = currentFrame.GetFileLineNumber();
        return this;
    }
}
