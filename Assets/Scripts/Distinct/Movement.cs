using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace distinct {
    public class Movement : MonoBehaviour
    {
        private CollisionNew coll;
        [HideInInspector]
        public Rigidbody2D rb;
        private AnimationScript anim;

        [Space]
        [Header("Stats")]
        public float speed = 10;
        public float jumpForce = 50;
        public float slideSpeed = 5;
        public float wallJumpLerp = 10;
        public float dashSpeed = 20;
        public float bufferTime = 1;
        public float coyoteTime = 1;
        public float slipperyRun = 1;

        [Space]
        [Header("Booleans")]
        public bool canMove;
        public bool jumpBuffer = false;
        public bool coyoteBuffer = false;
        public bool coyoteTriggered = false;
        public bool wallGrab;
        public bool wallJumped;
        public bool wallSlide;
        public bool isDashing;
        public bool hasJumped = false;


        //Initialize Publc Sound Effects, Assign Clips using Unity UI
        [Space]
        [Header("Audio")]
        public AudioSource jump;
        public AudioSource dash;
        public AudioSource land;
        public AudioSource run;
        public AudioSource wall;
        public AudioSource slide;


        [Space]

        private bool groundTouch;
        private bool hasDashed;

        public int side = 1;

        [Space]
        [Header("Polish")]
        public ParticleSystem dashParticle;
        public ParticleSystem jumpParticle;
        public ParticleSystem wallJumpParticle;
        public ParticleSystem slideParticle;

        // Start is called before the first frame update
        void Start()
        {
            coll = GetComponent<CollisionNew>();
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<AnimationScript>();
        }

        // Update is called once per frame
        void Update()
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            float xRaw = Input.GetAxisRaw("Horizontal");
            float yRaw = Input.GetAxisRaw("Vertical");
            Vector2 dir = new Vector2(x, y);

            Walk(dir);
            anim.SetHorizontalMovement(x, y, rb.velocity.y);

            if (!Input.GetKeyDown("s"))
            {
                if (Input.GetButtonDown("Jump"))
                {
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.99f);
                } else
                {
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.99f);
                }
                
            }

            //fast falling
            if (Input.GetKeyDown("s") && rb.velocity.y < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 2);
            }

            if (coll.onWall && Input.GetButton("Fire3") && canMove)
            {
                if (side != coll.wallSide)
                    anim.Flip(side * -1);
                wallGrab = true;
                wallSlide = false;


                if (slide.isPlaying)
                {
                    slide.Stop();
                }

                //Start running sound if it is not playing and if the character is moving
                if (rb.velocity.y != 0 && !wall.isPlaying)
                {
                    wall.Play();
                }
                //Stops running if its playing and the character isn't moving
                else if (rb.velocity.y == 0 && wall.isPlaying)
                {
                    wall.Stop();
                }
            }

            if (Input.GetButtonUp("Fire3") || !coll.onWall || !canMove)
            {
                wall.Stop(); //stop walking up wall sound
                wallGrab = false;
                wallSlide = false;
            }

            if (coll.onGround && !isDashing)
            {
                wallJumped = false;
                GetComponent<BetterJumping>().enabled = true;
            }

            if (wallGrab && !isDashing)
            {
                rb.gravityScale = 0;
                if (x > .2f || x < -.2f)
                    rb.velocity = new Vector2(rb.velocity.x, 0);

                float speedModifier = y > 0 ? .5f : 1;

                rb.velocity = new Vector2(rb.velocity.x, y * (speed * speedModifier));
            }
            else
            {
                rb.gravityScale = 0.7f;
            }

            if (coll.onWall && !Input.GetButton("Jump"))
            {
                if (x != 0 && !wallGrab)
                {
                    wallSlide = true;
                    WallSlide();
                }
            }

            if ((coll.onGround || !coll.onWall) && slide.isPlaying)
            {
                slide.Stop(); //stops slide sound from playing
            }

            if (!coll.onWall || coll.onGround)
                wallSlide = false;

            if (!hasJumped && !coll.onGround && !coyoteBuffer && !coyoteTriggered)
            {
                StartCoroutine(CoyoteTime());
            }

            if (Input.GetButtonDown("Jump"))
            {
                anim.SetTrigger("jump");

                if (coll.onGround)
                    Jump(Vector2.up, false);
                else
                {
                    if (coyoteBuffer)
                    {
                        Jump(Vector2.up, false);
                        coyoteBuffer = false;
                    }
                    else
                    {
                        StopCoroutine(JumpBuffer());
                        StartCoroutine(JumpBuffer());
                    }
                }

                if (coll.onWall && !coll.onGround && (wallSlide || wallGrab))
                    WallJump();
            }

            if (coll.onGround && jumpBuffer)
            {
                anim.SetTrigger("jump");
                if (coll.onGround)
                {
                    Jump(Vector2.up, false);
                    jumpBuffer = false;
                }
            }

            if (Input.GetButtonDown("Fire1") && !hasDashed)
            {
                if (xRaw != 0 || yRaw != 0)
                    Dash(xRaw, yRaw);
            }

            if (coll.onGround && !groundTouch)
            {
                GroundTouch();
                groundTouch = true;
            }

            if (!coll.onGround && groundTouch)
            {
                groundTouch = false;
            }

            WallParticle(y);

            if (wallGrab || wallSlide || !canMove)
                return;

            if (x > 0)
            {
                side = 1;
                anim.Flip(side);
            }
            if (x < 0)
            {
                side = -1;
                anim.Flip(side);
            }
        }

        void GroundTouch()
        {
            hasDashed = false;
            isDashing = false;
            hasJumped = false;
            coyoteBuffer = false;
            coyoteTriggered = false;

            side = anim.sr.flipX ? -1 : 1;

            //play Landing Sound
            land.Play();

            jumpParticle.Play();
        }

        private void Dash(float x, float y)
        {
            Camera.main.transform.DOComplete();
            Camera.main.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);
            FindObjectOfType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));

            //Play Dash Sound
            dash.Play();

            hasDashed = true;

            anim.SetTrigger("dash");

            rb.velocity = Vector2.zero;
            Vector2 dir = new Vector2(x, y);

            rb.velocity += dir.normalized * dashSpeed;
            StartCoroutine(DashWait());
        }

        IEnumerator DashWait()
        {
            FindObjectOfType<GhostTrailNew>().ShowGhost();
            StartCoroutine(GroundDash());
            DOVirtual.Float(14, 0, .8f, RigidbodyDrag);

            dashParticle.Play();
            //rb.gravityScale = 0;
            GetComponent<BetterJumping>().enabled = false;
            wallJumped = true;
            isDashing = true;

            yield return new WaitForSeconds(.3f);

            dashParticle.Stop();
            rb.gravityScale = 0.7f;
            GetComponent<BetterJumping>().enabled = true;
            wallJumped = false;
            isDashing = false;
        }

        IEnumerator GroundDash()
        {
            yield return new WaitForSeconds(.15f);
            if (coll.onGround)
                hasDashed = false;
        }

        private void WallJump()
        {
            if ((side == 1 && coll.onRightWall) || side == -1 && !coll.onRightWall)
            {
                side *= -1;
                anim.Flip(side);
            }

            StopCoroutine(DisableMovement(0));
            StartCoroutine(DisableMovement(.1f));

            Vector2 wallDir = coll.onRightWall ? Vector2.left : Vector2.right;

            Jump((Vector2.up / 1.5f + wallDir / 1.5f), true);

            wallJumped = true;
        }

        private void WallSlide()
        {
            //Only Play Slide if the sound isn't already playing
            if (!slide.isPlaying)
                slide.Play();

            if (coll.wallSide != side)
                anim.Flip(side * -1);

            if (!canMove)
            {
                slide.Stop(); //stops slide sound  
                return;
            }

            bool pushingWall = false;
            if ((rb.velocity.x > 0 && coll.onRightWall) || (rb.velocity.x < 0 && coll.onLeftWall))
            {
                pushingWall = true;
            }
            float push = pushingWall ? 0 : rb.velocity.x;

            rb.velocity = new Vector2(push, -slideSpeed);
        }

        private void Walk(Vector2 dir)
        {
            if (!canMove)
            {
                run.Stop(); //Stops Running Sound From Continuing to play
                return;
            }

            if (wallGrab)
            {
                run.Stop(); //Stops Running Sound From Continuing to play
                return;
            }

            if (coll.onLeftWall && dir.x < 0)
            {
                run.Stop(); //Stops Running Sound From Continuing to play
                return;
            }

            if (coll.onRightWall && dir.x > 0)
            {
                run.Stop(); //Stops Running Sound From Continuing to play
                return;
            }

            if (!wallJumped)
            {
                //Plays run if the sound isn't already playing, if the character is on the ground an if the character is moving
                if (!run.isPlaying && coll.onGround && rb.velocity.x != 0)
                {
                    run.Play();
                }
                //stops running sound for when the character walks off the platform without jumping
                else if (!coll.onGround)
                {
                    run.Stop(); //Stops Running Sound From Continuing to play
                }
                //Stops running if the character is just standing on the ground
                else if (coll.onGround && Input.GetAxis("Horizontal") == 0f)
                {
                    run.Stop(); //Stops Running Sound From Continuing to play
                }


                // rb.velocity = new Vector2(dir.x * speed, rb.velocity.y); // this is the original code
                rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), slipperyRun * Time.deltaTime );
            }
            else
            {
                run.Stop(); //Stops Running Sound From Continuing to play
                rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
            }
        }

        private void Jump(Vector2 dir, bool wall)
        {
            slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
            ParticleSystem particle = wall ? wallJumpParticle : jumpParticle;

            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.velocity += dir * jumpForce;

            hasJumped = true;

            //Play Jump Audio
            jump.Play();

            particle.Play();
        }

        IEnumerator DisableMovement(float time)
        {
            canMove = false;
            yield return new WaitForSeconds(time);
            canMove = true;
        }

        IEnumerator JumpBuffer()
        {
            jumpBuffer = true;
            yield return new WaitForSeconds(bufferTime);
            jumpBuffer = false;
        }

        IEnumerator CoyoteTime()
        {
            coyoteBuffer = true;
            yield return new WaitForSeconds(coyoteTime);
            coyoteBuffer = false;
            coyoteTriggered = true;
        }

        void RigidbodyDrag(float x)
        {
            rb.drag = x;
        }

        void WallParticle(float vertical)
        {
            var main = slideParticle.main;

            if (wallSlide || (wallGrab && vertical < 0))
            {
                slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
                main.startColor = Color.white;
            }
            else
            {
                main.startColor = Color.clear;
            }
        }

        int ParticleSide()
        {
            int particleSide = coll.onRightWall ? 1 : -1;
            return particleSide;
        }
    }

}

