using Unity.VisualScripting;
using UnityCTVisualizer;
using UnityEngine;

public static class TransferFunctionFactory {
    public static ITransferFunction Create(TF tf) {
        switch (tf) {
            case TF.TF1D:
            var tf_so = ScriptableObject.CreateInstance<TransferFunction1D>();
            tf_so.Init();
            return tf_so;

            default:
            throw new UnexpectedEnumValueException<TF>(tf);
        }
    }
}
