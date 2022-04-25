//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Dragon.Chess;

//namespace Dragon.Models
//{
//    public class Position
//    {
//        // Id and Encoded are the only things stored in the database
//        public int Id { get; set; }
//        public byte[] Encoded { get; set; }

//        // the following is only used in code
//        public sbyte[] Squares { get; set; }
//        public string Fen { get; set; }

//        //public int Store() {
//        //    string sql = "SELECT Id FROM Positions WHERE Encoded = @encoded";
//        //    byte[] bytes = ChessPositionEncoder.GetByteArrayForPosition(this);
//        //    int? id = Current.Db.Query<int?>(sql, new { encoded = bytes }).FirstOrDefault();

//        //    if (id.HasValue && id.Value > 0) {
//        //        this.Id = id.Value;
//        //        return id.Value;
//        //    }

//        //    // insert the position into the database
//        //    Current.Db.Execute("INSERT INTO Positions (Encoded) VALUES (@encoded)", new { encoded = bytes });

//        //    id = Current.Db.Query<int?>(sql, new { encoded = bytes }).FirstOrDefault();
//        //    if (id.HasValue && id.Value > 0) {
//        //        this.Id = id.Value;
//        //        return id.Value;
//        //    }

//        //    throw new Exception("Could not insert position into database");
//        //}
//    }
//}
