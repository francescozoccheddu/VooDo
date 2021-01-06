namespace VooDo.Runtime.Meta
{
    public interface ICallable
    {

        Eval Call(Env _env, Eval[] _arguments);

    }

}
