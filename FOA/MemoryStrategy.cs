using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FOA
{
    /// <summary>
    /// This class represent the memory allocation strategy.
    /// </summary>
    public class MemoryStrategy
    {
        /// <summary>
        /// Prefered initial buffer size.
        /// </summary>
        public const int DefaultInitSize = 128;

        /// <summary>
        /// Prefered reallocate buffer chunk size.
        /// </summary>
        public const int DefaultStepSize = 256;

        /// <summary>
        /// Prefered maximum buffer size.
        /// </summary>
        public const int DefaultMaxSize = 8 * 1024 * 1024;

        /// <summary>
        /// Alias for setting unlimited maximal buffer size.
        /// </summary>
        public const int Unlimited = 0;

        private int init;   // Initial buffer size.
        private int step;   // Realloc buffer step.
        private int max;    // Maximum buffer size.

        /// <summary>
        /// Creates the object using default sizes for initial buffer size, reallocation
        /// chunk size and maximum buffer size.
        /// </summary>
        public MemoryStrategy()
        {
            init = DefaultInitSize;
            step = DefaultStepSize;
            max = DefaultMaxSize;
        }

        /// <summary>
        /// Creates the object using the supplied sizes.
        /// </summary>
        /// <param name="init">The initial buffer size.</param>
        /// <param name="step">The buffer reallocation chunk size.</param>
        /// <param name="max">The maximum buffer size.</param>
        public MemoryStrategy(int init, int step, int max)
        {
            this.init = init;
            this.step = step;
            this.max = max;
        }

        /// <summary>
        /// Creates the object using default sizes for initial buffer size and reallocation
        /// chunk size, but with no maximum buffer size.
        /// </summary>
        /// <param name="init">The initial buffer size.</param>
        /// <param name="step">The buffer reallocation size.</param>
        public MemoryStrategy(int init, int step)
        {
            this.init = init;
            this.step = step;
            this.max = Unlimited;
        }

        /// <summary>
        /// Set/get the initial size of allocated buffer. Throws 
        /// ArgumentOutOfRangeException on value == 0.
        /// </summary>
        public int InitSize
        {
            get
            {
                return init;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                init = value;
            }
        }

        /// <summary>
        /// Set/get the buffer realloc chunk size. Throws 
        /// ArgumentOutOfRangeException on value == 0.
        /// </summary>
        public int StepSize
        {
            get
            {
                return step;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                step = value;
            }
        }

        /// <summary>
        /// Set/get the maximum buffer alloc size. Setting to 0 (MemoryAllocUnlim) 
        /// means buffer size is unlimited.
        /// </summary>
        public int MaxSize
        {
            get
            {
                return max;
            }

            set
            {
                max = value;
            }
        }

        /// <summary>
        /// Sets unlimited maximum buffer size.
        /// </summary>
        public void SetUnlimited()
        {
            max = Unlimited;
        }
    }
}
