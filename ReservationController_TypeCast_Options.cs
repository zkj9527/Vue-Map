// 方案1：如果 b.Type 是 int，c.Id 是 string
join c in _context.Fvenues on b.Type.ToString() equals c.Id into fvenueGroup

// 方案2：如果 b.Type 是 string，c.Id 是 int
join c in _context.Fvenues on b.Type equals c.Id.ToString() into fvenueGroup

// 方案3：如果两者都是数字类型但不同（如 int 和 long）
join c in _context.Fvenues on (long)b.Type equals c.Id into fvenueGroup

// 方案4：如果两者都是 int 类型，但可能是 nullable
join c in _context.Fvenues on b.Type equals c.Id into fvenueGroup

// 方案5：如果 b.Type 是 nullable int，c.Id 是 int
join c in _context.Fvenues on b.Type.Value equals c.Id into fvenueGroup

// 方案6：最安全的方式 - 都转换为 string
join c in _context.Fvenues on (b.Type?.ToString() ?? "") equals (c.Id?.ToString() ?? "") into fvenueGroup