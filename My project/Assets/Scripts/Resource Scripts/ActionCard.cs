[System.Serializable]

public class ActionCard
{
    public List<Effect> effects = new List<Effect>();

    public void applyAddEffects()
    {

    }
}

public enum EffectType
{
    RESOURCE,
    TILE
}

public class Effect
{
    public EffectType effectType;
    
    public void applyEffect()
    {

    }
}

public class Card
{
    public string name;
    public Player user;
    public Player target;

    public string getName()
    {
        return name;
    }

    public string setName(string newName)
    {
        name = newName;
        return name;      
    }


    public Player setUser()
    {
        return user;
    }

    public void setUser(Player newUser)
    {
        user = newUser;
    }
}