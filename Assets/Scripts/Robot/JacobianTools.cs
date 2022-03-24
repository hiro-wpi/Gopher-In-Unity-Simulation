using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JacobianTools
{
    public static void Print(ArticulationJacobian jacobian)
    {
        Debug.Log("Jacobian of size " + jacobian.rows + "x" + jacobian.columns);
        string jacobianString = "";
        for (int i = 0; i < jacobian.rows; i++)
        {
            string row = "";
            for (int j = 0; j < jacobian.columns; j++)
            {
                row += jacobian[i,j] + " ";
            }
            jacobianString += row + "\n";
        }
        Debug.Log(jacobianString);
    }
    public static List<float> Multiply(ArticulationJacobian jacobian, List<float> targetDelta)
    {
        List<float> result = new List<float>(jacobian.rows);
        for (int i = 0; i < jacobian.rows; i++)
        {
            result.Add(0.0f);
            for (int j = 0; j < jacobian.columns; j++)
            {
                result[i] += jacobian[i, j] * targetDelta[j];
            }
        }

        return result;
    }

    public static ArticulationJacobian Multiply(ArticulationJacobian jacobian1, ArticulationJacobian jacobian2)
    {
        if (jacobian1.columns != jacobian2.rows)
            throw new Exception("Can't multiply jacobians, jacobian1.columns != jacobian2.rows!");
        ArticulationJacobian result = new ArticulationJacobian(jacobian1.rows, jacobian2.columns);
        for (int row = 0; row < jacobian1.rows; row++)
            for (int column = 0; column < jacobian2.columns; column++)
                for (int i = 0; i < jacobian1.columns; i++)
                    result[row, column] += jacobian1[row, i] * jacobian2[i, column];
        return result;
    }

    public static ArticulationJacobian Multiply(ArticulationJacobian jacobian, float value)
    {
        ArticulationJacobian result = new ArticulationJacobian(jacobian.rows, jacobian.columns);
        for (int row = 0; row < jacobian.rows; row++)
            for (int column = 0; column < jacobian.columns; column++)
                result[row, column] = jacobian[row, column] * value;
        return result;
    }

    public static ArticulationJacobian Add(ArticulationJacobian jacobian1, ArticulationJacobian jacobian2)
    {
        if (jacobian1.rows != jacobian2.rows || jacobian1.columns != jacobian2.columns)
            throw new Exception("Can't add jacobians, matrix dimensions are not equal!");
        ArticulationJacobian result = new ArticulationJacobian(jacobian1.rows, jacobian1.columns);
        for (int row = 0; row < jacobian1.rows; row++)
            for (int column = 0; column < jacobian1.columns; column++)
                result[row, column] = jacobian1[row, column] + jacobian2[row, column];
        return result;
    }

    public static ArticulationJacobian Transpose(ArticulationJacobian jacobian)
    {
        ArticulationJacobian jacobianT = new ArticulationJacobian(jacobian.columns, jacobian.rows);
        for (int i = 0; i < jacobian.rows; i++)
        {
            for (int j = 0; j < jacobian.columns; j++)
                jacobianT[j, i] = jacobian[i, j];
        }

        return jacobianT;
    }

    public static ArticulationJacobian SwapRows(ArticulationJacobian jacobian, int row1, int row2)
    {
        if (row1 == row2)
            return jacobian;
        for (int i = 0; i < jacobian.columns; i++)
        {
            float temp = jacobian[row1, i];
            jacobian[row1, i] = jacobian[row2, i];
            jacobian[row2, i] = temp;
        }
        return jacobian;
    }

    // J* = (J.T*J)^-1*J.T
    public static ArticulationJacobian PsuedoInverse(ArticulationJacobian jacobian)
    {
        ArticulationJacobian jacobianT = Transpose(jacobian);
        ArticulationJacobian jacobianTjacobian = Multiply(jacobianT, jacobian);
        ArticulationJacobian jacobianTjacobianInv = Inverse(jacobianTjacobian);
        ArticulationJacobian jacobianTjacobianInvjacobianT = Multiply(jacobianTjacobianInv, jacobianT);
        return jacobianTjacobianInvjacobianT;
    }

    public static ArticulationJacobian Inverse(ArticulationJacobian jacobian)
    {
        const float deltaE = 1e-8f;
        if (jacobian.rows != jacobian.columns)
            throw new Exception("Can't find inverse for non square rows != columns jacobian!");
        ArticulationJacobian jacobianInv = new ArticulationJacobian(jacobian.rows, jacobian.columns);
        // Initialize to identity
        for (int diagonal = 0; diagonal < jacobianInv.rows; diagonal++)
            jacobianInv[diagonal, diagonal] = 1.0f;

        for (int diagonal = 0; diagonal < jacobian.rows; diagonal++)
        {
            int maxRow = diagonal;
            float maxValue = Math.Abs(jacobian[diagonal, diagonal]);
            float currentValue;
            for (int row = diagonal + 1; row < jacobian.rows; row++)
            {
                currentValue = Math.Abs(jacobian[row, diagonal]);
                if (currentValue > maxValue)
                {
                    maxRow = row;
                    maxValue = currentValue;
                }
            }
            if (maxValue < deltaE)
                throw new Exception("Jacobian is degenerate, can't compute inverse!");
            SwapRows(jacobian, diagonal, maxRow);
            SwapRows(jacobianInv, diagonal, maxRow);

            float inverseElement = 1 / jacobian[diagonal, diagonal];
            for (int col = diagonal; col < jacobian.columns; col++)
                jacobian[diagonal, col] *= inverseElement;
            for (int col = 0; col < jacobianInv.columns; col++)
                jacobianInv[diagonal, col] *= inverseElement;
            for (int row = 0; row < jacobian.rows; row++)
            {
                float value = jacobian[row, diagonal];
                if (row != diagonal)
                {
                    for (int column = diagonal; column < jacobian.columns; column++)
                        jacobian[row, column] -= value * jacobian[diagonal, column];
                    for (int column = 0; column < jacobianInv.columns; column++)
                        jacobianInv[row, column] -= value * jacobianInv[diagonal, column];

                }
            }

        }
        return jacobianInv;
    }

    public static void AssignIdentity(ref ArticulationJacobian jacobian)
    {
        for (int row = 0; row < jacobian.rows; row++)
        {
            for (int column = 0; column < jacobian.columns; column++)
                jacobian[row, column] = row != column ? 0.0f : 1.0f;
        }
    }

    public static ArticulationJacobian FillMatrix(int startRow, List<int> cols, ArticulationJacobian oldJ)
    {
        ArticulationJacobian minJ = new ArticulationJacobian(6, cols.Count);
        int row = -1;
        for (int r = startRow; r < startRow + 6; r++) {
            row += 1;
            int col = -1;
            foreach (int c in cols) {
                col += 1;
                minJ[row, col] = oldJ[r, c];
            }
        }
        return minJ;
    }
}
