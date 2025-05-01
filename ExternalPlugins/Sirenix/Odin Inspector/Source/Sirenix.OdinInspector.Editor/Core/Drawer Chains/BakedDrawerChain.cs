//-----------------------------------------------------------------------
// <copyright file="BakedDrawerChain.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
	using System.Collections;
	using System.Collections.Generic;
    using System.Linq;

    public class BakedDrawerChain : DrawerChain
    {
        private OdinDrawer[] bakedDrawerChain;
        private int index = -1;

        public BakedDrawerChain(InspectorProperty property, IEnumerable<OdinDrawer> chain)
            : base(property)
        {
            this.bakedDrawerChain = chain.ToArray();
        }

        public BakedDrawerChain(DrawerChain bakedChain)
            : base(bakedChain.Property)
        {
            this.BakedChain = bakedChain;
            this.Rebake();
        }

        public OdinDrawer[] BakedDrawerArray { get { return this.bakedDrawerChain; } }

        public DrawerChain BakedChain { get; private set; }

        public int CurrentIndex { get { return this.index; } }

        public override OdinDrawer Current
        {
            get
            {
                if (this.index >= 0 && this.index < this.bakedDrawerChain.Length)
                {
                    return this.bakedDrawerChain[index];
                }
                else
                {
                    return null;
                }
            }
        }

        public override bool MoveNext()
        {
            do
            {
                this.index++;

                if (this.Current != null)
                {
                    this.Property.IncrementDrawerChainIndex();
                }
            } while (this.Current != null && this.Current.SkipWhenDrawing);

            return this.Current != null;
        }

		public override IEnumerator<OdinDrawer> GetEnumerator()
		{
			return new BakedDrawerChainEnumerator(this);
		}

		public override void Reset()
        {
            this.index = -1;
        }

        public void Rebake()
        {
            if (this.BakedChain != null)
            {
                this.BakedChain.Reset();
                this.bakedDrawerChain = this.BakedChain.ToArray();
            }
        }
    }
    
    public struct BakedDrawerChainEnumerator : IEnumerator<OdinDrawer>
    {
		private BakedDrawerChain chain;
        private int index;
        private OdinDrawer current;

		public BakedDrawerChainEnumerator(BakedDrawerChain chain)
        {
            this.chain = chain;
            this.index = -1;
            this.current = null;
		}

		public OdinDrawer Current => current;
		object IEnumerator.Current => current;

		public void Dispose()
        {
		}

		public bool MoveNext()
        {
            if (this.index + 1 < this.chain.BakedDrawerArray.Length)
            {
				this.index++;
				this.current = this.chain.BakedDrawerArray[this.index];
				return true;
			}
			else
            {
				this.current = null;
				return false;
			}
		}

		public void Reset()
        {
            this.index = -1;
		}
	}

    public static partial class DrawerChainExtensions
    {
        public static BakedDrawerChain Bake(this DrawerChain chain)
        {
            if (chain == null) throw new ArgumentNullException("chain");

            var baked = chain as BakedDrawerChain;

            if (baked != null)
            {
                baked.Rebake();
                return baked;
            }
            else
            {
                return new BakedDrawerChain(chain);
            }
        }
    }
}
#endif