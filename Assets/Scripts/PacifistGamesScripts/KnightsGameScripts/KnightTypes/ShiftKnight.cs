using UnityEngine;

public class ShiftKnight : MonoBehaviour
{
    public int formIndex1;
    public int formIndex2;

    bool usingFirstForm = true;

    public KnightBehavior currentKnight;

    void Awake()
    {
        SetForms();
        CreateInitialForm();

        KnightsBoardManager.instance.shapeshifters.Add(this);
    }

    void SetForms()
    {
        var values = KnightsGameManager.instance.knightValues;

        formIndex1 = values[0];
        formIndex2 = values[1];
    }

    void CreateInitialForm()
    {
        currentKnight = CreateKnight(formIndex1);
    }

    public void ChangeForm()
    {
        int player = currentKnight.player;
        KnightsSquareScript square = currentKnight.currentSquare;
        KnightsBoardManager.instance.knightList.Remove(currentKnight);

        Destroy(currentKnight);

        int nextForm = usingFirstForm ? formIndex2 : formIndex1;

        currentKnight = CreateKnight(nextForm);

        KnightsBoardManager.instance.knightList.Add(currentKnight);

        currentKnight.player = player;
        currentKnight.currentSquare = square;

        if (square != null)
            square.knight = currentKnight;

        usingFirstForm = !usingFirstForm;
    }

    KnightBehavior CreateKnight(int value)
    {
        switch (value)
        {
            case 0: return gameObject.AddComponent<AgileKnight>();
            case 1: return gameObject.AddComponent<TucutuKnight>();
            case 2: return gameObject.AddComponent<BullKnight>();
            case 3: return gameObject.AddComponent<ShakyKnight>();
        }

        return gameObject.AddComponent<AgileKnight>();
    }
}