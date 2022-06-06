using System;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Sdk;

namespace OneBarker.WebOfStars.Tests;

public class Orbit_Should
{
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(100)]
    public void RequireSingleStepForFixedPoint(int badSteps)
    {
        var orbit = new OrbitalObject()
        {
            OrbitMajorWidth = 0,
            OrbitMinorWidth = 0,
            OrbitStepCount  = badSteps
        };

        var errors = orbit.GetErrors();
        Assert.False(orbit.IsValid());
        orbit.OrbitStepCount = 1;
        Assert.True(orbit.IsValid());
        
        Assert.Equal(1, errors.Count);
        Assert.Equal("OrbitStepCount", errors.Keys.First());
        Assert.Equal("must be one for a fixed point orbit", errors.Values.First()[0]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RequirePositiveStepForAnyOrbit(int badSteps)
    {
        var orbit = new OrbitalObject()
        {
            OrbitMajorWidth = 1,
            OrbitMinorWidth = 1,
            OrbitStepCount  = badSteps
        };
        
        var errors = orbit.GetErrors();
        Assert.False(orbit.IsValid());
        orbit.OrbitStepCount = 1;
        Assert.True(orbit.IsValid());
        orbit.OrbitStepCount = 10;
        Assert.True(orbit.IsValid());

        Assert.Equal(1, errors.Count);
        Assert.Equal("OrbitStepCount", errors.Keys.First());
        Assert.Equal("must be at least one for any orbit", errors.Values.First()[0]);
    }

    [Theory]
    [InlineData(1,1)]
    [InlineData(2,1)]
    [InlineData(1.5,1)]
    [InlineData(1,2)]
    [InlineData(1,1.1)]
    public void RequireMajorGteMinor(double major, double minor)
    {
        var orbit = new OrbitalObject()
        {
            OrbitMajorWidth = major,
            OrbitMinorWidth = minor,
            OrbitStepCount  = 4
        };
        if (major >= minor)
        {
            Assert.True(orbit.IsValid());
        }
        else
        {
            var errors = orbit.GetErrors();
            Assert.False(orbit.IsValid());
            
            Assert.True(errors.ContainsKey("OrbitMajorWidth"));
            Assert.True(errors.ContainsKey("OrbitMinorWidth"));
            Assert.Equal("must be greater than or equal to minor width", errors["OrbitMajorWidth"][0]);
            Assert.Equal("must be less than or equal to major width", errors["OrbitMinorWidth"][0]);
        }
    }

    [Theory]
    [InlineData(1,1)]
    [InlineData(1.1,1)]
    [InlineData(1.8,1)]
    [InlineData(2,1)]
    [InlineData(1,0.5)]
    public void AllowSafeMajorMinorRatio(double major, double minor)
    {
        var orbit = new OrbitalObject()
        {
            OrbitMajorWidth = major,
            OrbitMinorWidth = minor,
            OrbitStepCount  = 4
        };
        Assert.True(orbit.IsValid());
    }

    [Theory]
    [InlineData(2.1, 1)]
    [InlineData(1,0.1)]
    public void RejectUnsafeMajorMinorRatio(double major, double minor)
    {
        var orbit = new OrbitalObject()
        {
            OrbitMajorWidth = major,
            OrbitMinorWidth = minor,
            OrbitStepCount  = 4
        };
        var errors = orbit.GetErrors();
        Assert.False(orbit.IsValid());
        Assert.True(errors.ContainsKey("OrbitMajorWidth"));
        Assert.True(errors.ContainsKey("OrbitMinorWidth"));
        Assert.Equal("must have a ratio less than or equal to 2:1", errors["OrbitMajorWidth"][0]);
        Assert.Equal("must have a ratio less than or equal to 2:1", errors["OrbitMinorWidth"][0]);
    }

    [Fact]
    public void RejectNonFixedZeroWidth()
    {
        var orbit = new OrbitalObject()
        {
            OrbitStepCount  = 4,
            OrbitMajorWidth = 1,
            OrbitMinorWidth = 1
        };
        Assert.True(orbit.IsValid());
        orbit.OrbitMajorWidth = 0;
        Assert.False(orbit.IsValid());
        var errors = orbit.GetErrors();
        Assert.True(errors.ContainsKey("OrbitMajorWidth"));
        Assert.Equal("cannot be zero if minor width is not zero", errors["OrbitMajorWidth"][0]);
        orbit.OrbitMajorWidth = 1;
        Assert.True(orbit.IsValid());
        orbit.OrbitMinorWidth = 0;
        Assert.False(orbit.IsValid());
        errors = orbit.GetErrors();
        Assert.True(errors.ContainsKey("OrbitMinorWidth"));
        Assert.Equal("cannot be zero if major width is not zero", errors["OrbitMinorWidth"][0]);
        orbit.OrbitMinorWidth = 1;
        Assert.True(orbit.IsValid());
    }

    [Fact]
    public void RejectNegativeWidths()
    {
        var orbit = new OrbitalObject()
        {
            OrbitStepCount  = 4,
            OrbitMajorWidth = 1,
            OrbitMinorWidth = 1
        };
        Assert.True(orbit.IsValid());
        orbit.OrbitMajorWidth = -1;
        Assert.False(orbit.IsValid());
        var errors = orbit.GetErrors();
        Assert.True(errors.ContainsKey("OrbitMajorWidth"));
        Assert.Contains("cannot be negative", errors["OrbitMajorWidth"]);
        orbit.OrbitMajorWidth = 1;
        Assert.True(orbit.IsValid());
        orbit.OrbitMinorWidth = -1;
        Assert.False(orbit.IsValid());
        errors = orbit.GetErrors();
        Assert.True(errors.ContainsKey("OrbitMinorWidth"));
        Assert.Contains("cannot be negative", errors["OrbitMinorWidth"]);
        orbit.OrbitMinorWidth = 1;
        Assert.True(orbit.IsValid());
    }

    private bool EffectivelyEqual(double expected, double actual)
    {
        var eq = Math.Abs(actual - expected) < 0.0001;
        if (!eq) throw new EqualException(Math.Round(expected, 4), Math.Round(actual, 4));
        return eq;
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 0.5)]
    [InlineData(2, 1)]
    public void WorkAsExpected(double major, double minor)
    {
        var orbit = new OrbitalObject()
        {
            OrbitMajorWidth = major,
            OrbitMinorWidth = minor,
            OrbitStepCount  = 4,
            ClockwiseOrbit = true
        };
        
        Assert.True(orbit.IsValid());

        //
        //     +--
        //
        var pos = orbit.GetPositionForTurn(0);
        Assert.True(EffectivelyEqual(major, pos.X));
        Assert.True(EffectivelyEqual(0, pos.Y));
        
        //
        //     +
        //     |
        pos = orbit.GetPositionForTurn(1);
        Assert.True(EffectivelyEqual(0, pos.X));
        Assert.True(EffectivelyEqual(-minor, pos.Y));

        //
        //   --+
        //
        pos = orbit.GetPositionForTurn(2);
        Assert.True(EffectivelyEqual(-major, pos.X));
        Assert.True(EffectivelyEqual(0, pos.Y));
        
        //     |
        //     +
        //
        pos = orbit.GetPositionForTurn(3);
        Assert.True(EffectivelyEqual(0, pos.X));
        Assert.True(EffectivelyEqual(minor, pos.Y));

        orbit.ClockwiseOrbit = false;
        
        //
        //     +--
        //
        pos = orbit.GetPositionForTurn(0);
        Assert.True(EffectivelyEqual(major, pos.X));
        Assert.True(EffectivelyEqual(0, pos.Y));
        
        //     |
        //     +
        //     
        pos = orbit.GetPositionForTurn(1);
        Assert.True(EffectivelyEqual(0, pos.X));
        Assert.True(EffectivelyEqual(minor, pos.Y));

        //
        //   --+
        //
        pos = orbit.GetPositionForTurn(2);
        Assert.True(EffectivelyEqual(-major, pos.X));
        Assert.True(EffectivelyEqual(0, pos.Y));
        
        //     
        //     +
        //     |
        pos = orbit.GetPositionForTurn(3);
        Assert.True(EffectivelyEqual(0, pos.X));
        Assert.True(EffectivelyEqual(-minor, pos.Y));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 0.5)]
    [InlineData(2, 1)]
    public void Rotate90Degrees(double major, double minor)
    {
        // we can test 90 because we know exactly how it should behave.
        // it should also work with any other angle as well, but we can't test those as easily.
        var orbit = new OrbitalObject()
        {
            OrbitMajorWidth = major,
            OrbitMinorWidth = minor,
            OrbitStepCount  = 4,
            ClockwiseOrbit  = true,
            OrbitRotation = 90.DegreesToRadians()
        };
        
        Assert.True(orbit.IsValid());
        
        //     |
        //     +
        //
        var pos = orbit.GetPositionForTurn(0);
        Assert.True(EffectivelyEqual(0, pos.X));
        Assert.True(EffectivelyEqual(major, pos.Y));
        
        
        //
        //     +--
        //     
        pos = orbit.GetPositionForTurn(1);
        Assert.True(EffectivelyEqual(minor, pos.X));
        Assert.True(EffectivelyEqual(0, pos.Y));

        //
        //     +
        //     |
        pos = orbit.GetPositionForTurn(2);
        Assert.True(EffectivelyEqual(0, pos.X));
        Assert.True(EffectivelyEqual(-major, pos.Y));
        
        //     
        //   --+
        //
        pos = orbit.GetPositionForTurn(3);
        Assert.True(EffectivelyEqual(-minor, pos.X));
        Assert.True(EffectivelyEqual(0, pos.Y));
    }

    [Fact]
    public void WorkNested()
    {
        var p = new OrbitalObject()
        {
            OrbitMajorWidth = 1,
            OrbitMinorWidth = 1,
            OrbitStepCount  = 4,
            ClockwiseOrbit  = true
        };
        var orbit = new OrbitalObject()
        {
            OrbitMajorWidth = 1,
            OrbitMinorWidth = 1,
            OrbitStepCount  = 4,
            OrbitStepOffset = 1,
            ClockwiseOrbit  = false,
            Parent = p
        };

        Assert.True(orbit.IsValid());
        
        //       o
        //       |
        //    +--+
        //       
        //
        var pos = orbit.GetPositionForTurn(0);
        Assert.True(EffectivelyEqual(1, pos.X));
        Assert.True(EffectivelyEqual(1, pos.Y));
        
        //
        //
        //    +
        //    |
        // o--+
        pos = orbit.GetPositionForTurn(1);
        Assert.True(EffectivelyEqual(-1, pos.X));
        Assert.True(EffectivelyEqual(-1, pos.Y));
        
        //
        //
        // +--+
        // |
        // o
        pos = orbit.GetPositionForTurn(2);
        Assert.True(EffectivelyEqual(-1, pos.X));
        Assert.True(EffectivelyEqual(-1, pos.Y));
        
        //    +--o
        //    |
        //    +
        //
        //
        pos = orbit.GetPositionForTurn(3);
        Assert.True(EffectivelyEqual(1, pos.X));
        Assert.True(EffectivelyEqual(1, pos.Y));
    }

    [Fact]
    public void NotOverflowOnSteps()
    {
        var orbit = new OrbitalObject()
        {
            OrbitMajorWidth = 1,
            OrbitMinorWidth = 1,
            OrbitStepCount  = 20,
            ClockwiseOrbit  = true
        };

        Assert.True(orbit.IsValid());
        
        for (var i = 0; i < 1000; i++)
        {
            var pos = orbit.GetPositionForTurn(i);

            // each 5 steps we should be at one of the known points.
            if (i % 5 == 0)
            {
                // select the known point.
                var dir = (i / 5) % 4;
                var (ex, ey) = dir switch
                {
                    0 => (1.0, 0.0),
                    1 => (0.0, -1.0),
                    2 => (-1.0, 0.0),
                    3 => (0.0, 1.0),
                    _ => (0, 0)
                };
                
                // and test for equality.
                Assert.True(EffectivelyEqual(ex, pos.X));
                Assert.True(EffectivelyEqual(ey, pos.Y));
            }
            
        }
        
        
    }
    
}
