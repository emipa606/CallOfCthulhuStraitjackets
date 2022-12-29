using Verse;

namespace Cthulhu.NoCCL;

public class SpecialInjector
{
    public virtual bool Inject()
    {
        Log.Error("This should never be called.");
        return false;
    }
}