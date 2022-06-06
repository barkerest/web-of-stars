using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OneBarker.WebOfStars;

public class OrbitalObject : IValidatableObject
{
    /// <summary>
    /// Maximum number of supported steps per orbit.
    /// </summary>
    public static readonly int MaximumStepCount = 2000;

    private Position[]? _steps;

    private double _orbitRotation;
    private double _orbitOffsetX;
    private double _orbitOffsetY;
    private double _orbitMajorWidth;
    private double _orbitMinorWidth;
    private int    _orbitStepCount;
    private int    _orbitStepOffset;
    private bool   _clockwiseOrbit;

    /// <summary>
    /// The ID for this orbital object.
    /// </summary>
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ID { get; set; }

    /// <summary>
    /// The ID for the object this object is offset by.
    /// </summary>
    /// <remarks>
    /// The current position in the parent orbit is the starting point for the origin of this orbit.
    /// If no parent is set, then the starting point is at (0, 0).
    /// The OffsetX and OffsetY properties are added to the starting point to compute the origin for this orbit.
    /// </remarks>
    public long? ParentID { get; set; }

    /// <summary>
    /// The object this object is offset by.
    /// </summary>
    [ForeignKey(nameof(ParentID))]
    public OrbitalObject? Parent { get; set; }

    /// <summary>
    /// The name of this object.
    /// </summary>
    [StringLength(120)]
    public string? Name { get; set; }

    /// <summary>
    /// The orbital rotation in radians.
    /// </summary>
    public double OrbitRotation
    {
        get => _orbitRotation;
        set
        {
            _orbitRotation = value;
            _steps    = null;
        }
    }

    /// <summary>
    /// The X offset for the orbit origin.
    /// </summary>
    public double OrbitOffsetX
    {
        get => _orbitOffsetX;
        set
        {
            _orbitOffsetX = value;
            _steps   = null;
        }
    }

    /// <summary>
    /// The Y offset for the orbit origin.
    /// </summary>
    public double OrbitOffsetY
    {
        get => _orbitOffsetY;
        set
        {
            _orbitOffsetY = value;
            _steps   = null;
        }
    }

    /// <summary>
    /// The major width of the orbit.
    /// </summary>
    /// <remarks>
    /// If Major and Minor are both 0, then the orbit represents a fixed point.
    /// The ratio between Major and Minor should never exceed 2:1.
    /// </remarks>
    public double OrbitMajorWidth
    {
        get => _orbitMajorWidth;
        set
        {
            _orbitMajorWidth = value;
            _steps      = null;
        }
    }

    /// <summary>
    /// The minor width of the orbit.
    /// </summary>
    /// <remarks>
    /// If Major and Minor are both 0, then the orbit represents a fixed point.
    /// The ratio between Major and Minor should never exceed 2:1.
    /// </remarks>
    public double OrbitMinorWidth
    {
        get => _orbitMinorWidth;
        set
        {
            _orbitMinorWidth = value;
            _steps      = null;
        }
    }

    /// <summary>
    /// The number of steps to complete an orbit.
    /// </summary>
    public int OrbitStepCount
    {
        get => _orbitStepCount;
        set
        {
            _orbitStepCount = value;
            _steps     = null;
        }
    }

    /// <summary>
    /// The offset for the steps of the orbit.
    /// </summary>
    /// <remarks>
    /// Adjusts the starting position in the orbit.
    /// </remarks>
    public int OrbitStepOffset
    {
        get => _orbitStepOffset;
        set
        {
            _orbitStepOffset = value;
            _steps      = null;
        }
    }

    /// <summary>
    /// Indicates if the orbit is clockwise (true) or counter-clockwise (false).
    /// </summary>
    public bool ClockwiseOrbit
    {
        get => _clockwiseOrbit;
        set
        {
            _clockwiseOrbit = value;
            _steps     = null;
        }
    }

    private Position[] Steps
    {
        get
        {
            if (_steps != null) return _steps;

            _steps = this.IsValid() ? GenerateSteps() : Array.Empty<Position>();

            return _steps;
        }
    }

    /// <summary>
    /// Gets the position on this orbit for the supplied turn number.
    /// </summary>
    /// <param name="turn">The turn (0 to 2,147,483,647).</param>
    /// <returns>Returns the position.</returns>
    public Position GetPositionForTurn(int turn)
    {
        if (turn < 0) turn = 0;
        if (Steps.Length != OrbitStepCount) return new Position();  // invalid config.

        var offset = Parent?.GetPositionForTurn(turn) ?? new Position();

        offset = new Position(offset.X + OrbitOffsetX, offset.Y + OrbitOffsetY);

        var pos = OrbitStepCount == 1 ? Steps[0] : Steps[turn % OrbitStepCount];
        
        return new Position(offset.X + pos.X, offset.Y + pos.Y);
    }

    private Position[] GenerateSteps()
    {
        var ret = new Position[OrbitStepCount];

        // fixed point.
        if (OrbitMajorWidth == 0)
        {
            return new[] { new Position(OrbitOffsetX, OrbitOffsetY) };
        }

        var dir        = ClockwiseOrbit ? -1.0 : 1.0;
        var radPerStep = dir * (2 * Math.PI) / OrbitStepCount;
        var rad        = radPerStep * OrbitStepOffset;

        // TODO: Double check matrix math.
        var sinR = Math.Sin(OrbitRotation);
        var cosR = Math.Cos(OrbitRotation);
        
        for (var i = 0; i < _orbitStepCount; i++)
        {
            // compute position.
            var x = Math.Cos(rad) * OrbitMajorWidth;
            var y = Math.Sin(rad) * OrbitMinorWidth;

            // perform rotation and set the value.
            ret[i] = new Position(
                x * cosR - y * sinR,
                x * sinR + y * cosR);

            // move to the next step.
            rad += radPerStep;
        }

        return ret;
    }

    IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
    {
        if (OrbitMajorWidth == 0 &&
            OrbitMinorWidth == 0)
        {
            if (OrbitStepCount != 1) yield return new ValidationResult("must be one for a fixed point orbit", new[] { nameof(OrbitStepCount) });
        }
        else if (OrbitMajorWidth == 0)
        {
            yield return new ValidationResult("cannot be zero if minor width is not zero", new[] { nameof(OrbitMajorWidth) });
        }
        else if (OrbitMinorWidth == 0)
        {
            yield return new ValidationResult("cannot be zero if major width is not zero", new[] { nameof(OrbitMinorWidth) });
        }

        if (OrbitMajorWidth < 0) yield return new ValidationResult("cannot be negative", new[] { nameof(OrbitMajorWidth) });
        if (OrbitMinorWidth < 0) yield return new ValidationResult("cannot be negative", new[] { nameof(OrbitMinorWidth) });

        if (OrbitMajorWidth < OrbitMinorWidth)
        {
            yield return new ValidationResult("must be greater than or equal to minor width", new[] { nameof(OrbitMajorWidth) });
            yield return new ValidationResult("must be less than or equal to major width", new[] { nameof(OrbitMinorWidth) });
        }

        if (OrbitMinorWidth > 0 &&
            OrbitMajorWidth > 0)
        {
            if (OrbitMajorWidth >= OrbitMinorWidth)
            {
                var ratio = OrbitMajorWidth / OrbitMinorWidth;
                if (ratio > 2.0) yield return new ValidationResult("must have a ratio less than or equal to 2:1", new[] { nameof(OrbitMajorWidth), nameof(OrbitMinorWidth) });
            }

            if (OrbitStepCount < 1) yield return new ValidationResult("must be at least one for any orbit", new[] { nameof(OrbitStepCount) });
            if (OrbitStepCount > MaximumStepCount) yield return new ValidationResult($"must be less than or equal to {MaximumStepCount} for any orbit", new[] { nameof(OrbitStepCount) });
        }
    }
    
    
    
}
