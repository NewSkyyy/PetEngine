using System.Numerics;

public class VertexOperations
{
    private static Vector3 translationStack = new Vector3(0.0f, 0.0f, 0.0f);
    
    private static Vector3 angleStack;

    //For now Move3 do not include z axis
    public static float[] Move3(float[] startMatrix, float direction, float magnitude, uint stride)
    {
        float [] resMatrix = startMatrix;
        
        // Absulute translation
        Vector3 dirToVector = new Vector3 (MathF.Sin(direction) * magnitude, MathF.Cos(direction) * magnitude, 0.0f);
        // Relative translation (only with global rotation)
        //Vector3 dirToVector = new Vector3 (MathF.Sin(direction-angleStack.Z) * magnitude, MathF.Cos(direction-angleStack.Z) * magnitude, 0.0f);

        translationStack += dirToVector;

        for (int i = 0; i < resMatrix.Length; i += (int) stride)
        {
            (resMatrix[i], resMatrix[i+1],resMatrix[i+2]) = (resMatrix[i] + dirToVector.X, resMatrix[i+1]+ dirToVector.Y,resMatrix[i+2] + dirToVector.Z);
        }
        
        return resMatrix;
    }

    // For now do not use "global" centerPoint
    public static float[] Rotate3(float[] startMatrix, float angle,  uint stride, string axis = "Z", string centerPoint = "object")
    {
        float [] resMatrix = startMatrix;
        // Do no know why this needs -angle
        Vector3 dirToVector = new Vector3 (MathF.Sin(-angle), MathF.Cos(-angle), 0.0f);
        Vector3 objectStack;
        
        int[] axisStep = {0, 1, 2};

        switch (axis)
        {
            case "X":
                axisStep = new int[] {1, 2, 0};
                break;
            case "Y":
                axisStep = new int[] {2, 0, 1};
                break;
            case "Z":
                axisStep = new int[] {0, 1, 2};
                break;
            default:
                break;
        }

        if (centerPoint == "object")
            objectStack = translationStack;
        else if (centerPoint == "global")
        {
            throw new Exception("\"Global\" centerPoint do not work properly for now");
            objectStack = new Vector3(0.0f, 0.0f, 0.0f);
            angleStack[axisStep[2]] += angle;
        }
        else
            throw new Exception($"Center point \"{centerPoint}\" is not supported");
        
        for (int i = 0; i < resMatrix.Length; i += (int) stride)
        { 
            if (centerPoint == "object")
                (resMatrix[i+axisStep[0]], resMatrix[i+axisStep[1]],resMatrix[i+axisStep[2]]) = (resMatrix[i+axisStep[0]] * MathF.Cos(angleStack[axisStep[2]]) - resMatrix[i+axisStep[1]] * MathF.Sin(angleStack[axisStep[2]]), resMatrix[i+axisStep[0]] * MathF.Sin(angleStack[axisStep[2]]) + resMatrix[i+axisStep[1]] * MathF.Cos(angleStack[axisStep[2]]), resMatrix[i+axisStep[2]]);
            (resMatrix[i], resMatrix[i+1],resMatrix[i+2]) = (resMatrix[i] - objectStack.X, resMatrix[i+1] - objectStack.Y, resMatrix[i+2]- objectStack.Z);
            (resMatrix[i+axisStep[0]], resMatrix[i+axisStep[1]],resMatrix[i+axisStep[2]]) = (resMatrix[i+axisStep[0]] * dirToVector.Y - resMatrix[i+axisStep[1]] * dirToVector.X, resMatrix[i+axisStep[0]] * dirToVector.X + resMatrix[i+axisStep[1]] * dirToVector.Y, resMatrix[i+axisStep[2]]);
            (resMatrix[i], resMatrix[i+1],resMatrix[i+2]) = (resMatrix[i] + objectStack.X, resMatrix[i+1] + objectStack.Y, resMatrix[i+2] + objectStack.Z);
            if (centerPoint == "object")
                (resMatrix[i+axisStep[0]], resMatrix[i+axisStep[1]],resMatrix[i+axisStep[2]]) = (resMatrix[i+axisStep[0]] * MathF.Cos(-angleStack[axisStep[2]]) - resMatrix[i+axisStep[1]] * MathF.Sin(-angleStack[axisStep[2]]), resMatrix[i+axisStep[0]] * MathF.Sin(-angleStack[axisStep[2]]) + resMatrix[i+axisStep[1]] * MathF.Cos(-angleStack[axisStep[2]]), resMatrix[i+axisStep[2]]);
        }
        
        return resMatrix;
    }
    public static float[] Scale3(float[] startMatrix, float scale,  uint stride)
    {
        float [] resMatrix = startMatrix;
        
        for (int i = 0; i < resMatrix.Length; i += (int) stride)
        {
            (resMatrix[i], resMatrix[i+1],resMatrix[i+2]) = (resMatrix[i] - translationStack.X, resMatrix[i+1] - translationStack.Y, resMatrix[i+2]- translationStack.Z);
            (resMatrix[i], resMatrix[i+1],resMatrix[i+2]) = (resMatrix[i] * scale, resMatrix[i+1] * scale, resMatrix[i+2] * scale);
            (resMatrix[i], resMatrix[i+1],resMatrix[i+2]) = (resMatrix[i] + translationStack.X, resMatrix[i+1] + translationStack.Y, resMatrix[i+2]+ translationStack.Z);
        }
       
        return resMatrix;
    }

    //Sample
    //{
    ////     aPosition       |   aTexCoords
    //     0.5f,  0.5f, 0.0f,  1.0f, 1.0f,
    //     0.5f, -0.5f, 0.0f,  1.0f, 0.0f, 
    //    -0.5f, -0.5f, 0.0f,  0.0f, 0.0f,
    //    -0.5f,  0.5f, 0.0f,  0.0f, 1.0f
    //};
    //
}