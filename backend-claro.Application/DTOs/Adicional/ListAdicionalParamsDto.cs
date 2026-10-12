
public class  ListAdicionalParamsDto
{
    public int NumPagina {get; set;} = 1 ;
    public int CanMaxPag {get; set;} = 50;

    private int _canPag {get; set;} =  15;

    public int CantidadPagina 
    {
        get => _canPag;
        set => _canPag =(value <= CanMaxPag) ? CanMaxPag : (value <= 0 ? 10 : value);
    }

    public string Rol{get; set;} = String.Empty;
    

}