namespace PromoRandom.Models;

public class User
{
    public int Id { get; set; }
    public string? Language { get; set; }  // ����, ����� ���� null, ���� �� ������
    public string? Name { get; set; }      // ��� ������������
    public string? Phone { get; set; }     // �������
    public long ChatId { get; set; }       // ��������-������������� ������������
}