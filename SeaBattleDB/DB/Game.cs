using System;
using System.Collections.Generic;

namespace SeaBattleDB.DB;

public partial class Game
{
    public int Id { get; set; }

    public byte[] FieldUser1 { get; set; } = null!;

    public byte[] FieldUser2 { get; set; } = null!;

    /// <summary>
    /// 0 start, 1 process, 2 end
    /// </summary>
    public short Status { get; set; }

    public int IdUserNextTurn { get; set; }

    public DateTime DatetimeStartGame { get; set; }

    public DateTime? DatetimeLastTurn { get; set; }

    public int? IdUserWinner { get; set; }

    public int CreatorUserId { get; set; }

    public virtual ICollection<User> IdUsers { get; set; } = new List<User>();
}
