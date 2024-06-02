namespace C_Sharp_lab_4.Models
{
    public class User
    {
        /// <summary>
        /// Порядковый номре пользователя в БД
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Логин пользователя
        /// </summary>
        public string Login { get; set; } = null!;
        /// <summary>
        /// Пароль пользователя
        /// </summary>
        public string Password { get; set; } = null!;
        /// <summary>
        /// ФИО пользователя
        /// </summary>
        public string FIO {  get; set; } = null!;
    }
}
