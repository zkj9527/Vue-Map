using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// 多条件查询预约（可筛选日期）
/// </summary>
/// <param name="query">查询参数</param>
/// <returns>返回符合条件的预约列表</returns>
/// <response code="200">查询成功</response>
/// <response code="400">用户ID无效</response>
[HttpGet("query")]
[ProducesResponseType(typeof(List<ReservationWithVenueType>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[SwaggerOperation(
    Summary = "多条件查询预约"
)]
public async Task<ActionResult<IEnumerable<ReservationWithVenueType>>> GetReservationsByUser(
    [FromQuery, SwaggerParameter("查询条件", Required = true)] QueryReservationsDto query)
{
    // 构建基础查询 - 对应您的 SQL：
    // SELECT d.venue_type,a.* FROM dbo.reservations as a 
    // left join (select c.venue_type,b.* from dbo.venues as b left join dbo.fvenues as c
    // on b.type=c.id) as d on a.venue_id = d.venue_id
    
    var baseQuery = from a in _context.Reservations
                    join venueWithType in (
                        from b in _context.Venues
                        join c in _context.Fvenues on (b.Type?.ToString() ?? "") equals (c.Id?.ToString() ?? "") into fvenueGroup
                        from c in fvenueGroup.DefaultIfEmpty()
                        select new { 
                            Venue_Type = c != null ? c.Venue_Type : null,
                            Venue = b
                        }
                    ) on a.Venue_Id equals venueWithType.Venue.Venue_Id into venueData
                    from d in venueData.DefaultIfEmpty()
                    select new ReservationWithVenueType
                    {
                        // d.venue_type
                        Venue_Type = d != null ? d.Venue_Type : null,
                        
                        // a.* (所有预约字段)
                        Reservation_Id = a.Reservation_Id,
                        User_Id = a.User_Id,
                        Venue_Id = a.Venue_Id,
                        Start_Time = a.Start_Time,
                        End_Time = a.End_Time,
                        Venue_Name = a.Venue_Name,
                        
                        // 原始预约对象，如果需要其他字段
                        Reservation = a
                    };

    // 如果传了用户id：按用户ID过滤
    if (query.User_Id.HasValue)
    {
        baseQuery = baseQuery
            .Where(r => r.User_Id == query.User_Id);
    }

    // 如果传了日期，添加日期范围过滤
    if (query.Date.HasValue)
    {
        var date = query.Date.Value.Date; // 提取日期部分
        var startDate = date;
        var endDate = date.AddDays(1).AddSeconds(-1);

        baseQuery = baseQuery
            .Where(r => r.Start_Time >= startDate && r.End_Time <= endDate);
    }

    // 如果传了场地ID，添加场地id范围过滤
    if (query.Venue_Id.HasValue)
    {
        baseQuery = baseQuery
            .Where(r => r.Venue_Id == query.Venue_Id);
    }

    // 按开始时间排序
    var result = await baseQuery
        .OrderBy(r => r.Start_Time)
        .ToListAsync();

    return Ok(new ApiResponse<List<ReservationWithVenueType>>
    {
        Success = true,
        Data = result,
        Message = result.Count > 0
            ? "查询成功"
            : "未找到符合条件的预约"
    });
}

// 返回数据的DTO类
public class ReservationWithVenueType
{
    /// <summary>
    /// 场地类型 (来自 fvenues 表)
    /// </summary>
    public string Venue_Type { get; set; }
    
    /// <summary>
    /// 预约ID
    /// </summary>
    public int Reservation_Id { get; set; }
    
    /// <summary>
    /// 用户ID
    /// </summary>
    public int? User_Id { get; set; }
    
    /// <summary>
    /// 场地ID
    /// </summary>
    public int? Venue_Id { get; set; }
    
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime Start_Time { get; set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime End_Time { get; set; }
    
    /// <summary>
    /// 场地名称
    /// </summary>
    public string Venue_Name { get; set; }
    
    /// <summary>
    /// 完整的预约对象（如果需要访问其他预约字段）
    /// </summary>
    public Reservation Reservation { get; set; }
}