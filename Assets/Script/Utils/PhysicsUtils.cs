using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MMD_URP.PMXRigidbody;

namespace MMD_URP
{
    public class PhysicsUtils
    {
        public static int GetRelBoneIndexFromNearbyRigidbody(int rigidbody_index, PMXRigidbody[] rigidBodies, PMXBone[] bones, PMXJoint[] joints)
        {
            int bone_count = bones.Length;
            //�v�B�ܩ`���̽��
            int result = rigidBodies[rigidbody_index].rel_bone_index;
            if (result < bone_count)
            {
                //�v�B�ܩ`���Ф��
                return result;
            }
            //�v�B�ܩ`�󤬟o�����
            //���祤��Ȥ˽ӾA����Ƥ��넂����v�B�ܩ`���̽�����Ф�
            //HACK: �����̽���˞�äƤ��뤱��ɡ��v�B�ܩ`��Ȥ�����Ԥ򿼤���з�����̽���η���������˼����

            //���祤��Ȥ�A��̽�����Ф�(������B�˽ӾA����Ƥ���)
            var joint_a_list = joints.Where(x => x.rigidbody_b == rigidbody_index) //������B�˽ӾA����Ƥ��른�祤��Ȥ˽g��
                                                                .Where(x => x.rigidbody_a < bone_count) //A���Є��ʄ���˿`��
                                                                .Select(x => x.rigidbody_a); //A�򷵤�
            foreach (var joint_a in joint_a_list)
            {
                result = GetRelBoneIndexFromNearbyRigidbody(joint_a, rigidBodies, bones, joints);
                if (result < bone_count)
                {
                    //�v�B�ܩ`���Ф��
                    return result;
                }
            }
            //���祤��Ȥ�A�˟o�����
            //���祤��Ȥ�B��̽�����Ф�(������A�˽ӾA����Ƥ���)
            var joint_b_list = joints.Where(x => x.rigidbody_a == rigidbody_index) //������A�˽ӾA����Ƥ��른�祤��Ȥ˽g��
                                                                .Where(x => x.rigidbody_b < bone_count) //B���Є��ʄ���˿`��
                                                                .Select(x => x.rigidbody_b); //B�򷵤�
            foreach (var joint_b in joint_b_list)
            {
                result = GetRelBoneIndexFromNearbyRigidbody(joint_b, rigidBodies, bones, joints);
                if (result < bone_count)
                {
                    //�v�B�ܩ`���Ф��
                    return result;
                }
            }
            //����Ǥ�o�����
            //�B���
            result = -1;
            return result;
        }

        public static void UnityRigidbodySetting(PMXRigidbody pmx_rigidbody, GameObject target)
        {
            Rigidbody rigidbody = target.GetComponent<Rigidbody>();
            if (null != rigidbody)
            {
                //�Ȥ�Rigidbody�����뤵��Ƥ���ʤ�
                //�|���Ϻ��㤹��
                rigidbody.mass += pmx_rigidbody.weight;
                //�p˥����ƽ����ȡ��
                rigidbody.linearDamping = (rigidbody.linearDamping + pmx_rigidbody.position_dim) * 0.5f;
                rigidbody.angularDamping = (rigidbody.angularDamping + pmx_rigidbody.rotation_dim) * 0.5f;
            }
            else
            {
                //�ޤ�Rigidbody�����뤵��Ƥ��ʤ��ʤ�
                rigidbody = target.AddComponent<Rigidbody>();
                rigidbody.isKinematic = pmx_rigidbody.operation_type == OperationType.Static;
                rigidbody.mass = Mathf.Max(float.Epsilon, pmx_rigidbody.weight);
                rigidbody.linearDamping = pmx_rigidbody.position_dim;
                rigidbody.angularDamping = pmx_rigidbody.rotation_dim;
            }
        }

        public static void SetMotionAngularLock(PMXJoint joint, ConfigurableJoint conf)
        {
            SoftJointLimit jlim;

            // Motion�ι̶�
            if (joint.constrain_pos_lower.x == 0.0f && joint.constrain_pos_upper.x == 0.0f)
            {
                conf.xMotion = ConfigurableJointMotion.Locked;
            }
            else
            {
                conf.xMotion = ConfigurableJointMotion.Limited;
            }

            if (joint.constrain_pos_lower.y == 0.0f && joint.constrain_pos_upper.y == 0.0f)
            {
                conf.yMotion = ConfigurableJointMotion.Locked;
            }
            else
            {
                conf.yMotion = ConfigurableJointMotion.Limited;
            }

            if (joint.constrain_pos_lower.z == 0.0f && joint.constrain_pos_upper.z == 0.0f)
            {
                conf.zMotion = ConfigurableJointMotion.Locked;
            }
            else
            {
                conf.zMotion = ConfigurableJointMotion.Limited;
            }

            // �ǶȤι̶�
            if (joint.constrain_rot_lower.x == 0.0f && joint.constrain_rot_upper.x == 0.0f)
            {
                conf.angularXMotion = ConfigurableJointMotion.Locked;
            }
            else
            {
                conf.angularXMotion = ConfigurableJointMotion.Limited;
                float hlim = Mathf.Max(-joint.constrain_rot_lower.x, -joint.constrain_rot_upper.x); //��ܞ������ʤΤ�ؓ��
                float llim = Mathf.Min(-joint.constrain_rot_lower.x, -joint.constrain_rot_upper.x);
                SoftJointLimit jhlim = new SoftJointLimit();
                jhlim.limit = Mathf.Clamp(hlim * Mathf.Rad2Deg, -180.0f, 180.0f);
                conf.highAngularXLimit = jhlim;

                SoftJointLimit jllim = new SoftJointLimit();
                jllim.limit = Mathf.Clamp(llim * Mathf.Rad2Deg, -180.0f, 180.0f);
                conf.lowAngularXLimit = jllim;
            }

            if (joint.constrain_rot_lower.y == 0.0f && joint.constrain_rot_upper.y == 0.0f)
            {
                conf.angularYMotion = ConfigurableJointMotion.Locked;
            }
            else
            {
                // �����ޥ��ʥ����ȥ���`������Τ�ע��
                conf.angularYMotion = ConfigurableJointMotion.Limited;
                float lim = Mathf.Min(Mathf.Abs(joint.constrain_rot_lower.y), Mathf.Abs(joint.constrain_rot_upper.y));//�~������С������
                jlim = new SoftJointLimit();
                jlim.limit = lim * Mathf.Clamp(Mathf.Rad2Deg, 0.0f, 180.0f);
                conf.angularYLimit = jlim;
            }

            if (joint.constrain_rot_lower.z == 0f && joint.constrain_rot_upper.z == 0f)
            {
                conf.angularZMotion = ConfigurableJointMotion.Locked;
            }
            else
            {
                conf.angularZMotion = ConfigurableJointMotion.Limited;
                float lim = Mathf.Min(Mathf.Abs(-joint.constrain_rot_lower.z), Mathf.Abs(-joint.constrain_rot_upper.z));//�~������С������//��ܞ������ʤΤ�ؓ��
                jlim = new SoftJointLimit();
                jlim.limit = Mathf.Clamp(lim * Mathf.Rad2Deg, 0.0f, 180.0f);
                conf.angularZLimit = jlim;
            }
        }

        public static void SetDrive(PMXJoint joint, ConfigurableJoint conf)
        {
            JointDrive drive;

            // Position
            if (joint.spring_position.x != 0.0f)
            {
                drive = new JointDrive();
                drive.positionSpring = joint.spring_position.x * 1.0f;
                conf.xDrive = drive;
            }
            if (joint.spring_position.y != 0.0f)
            {
                drive = new JointDrive();
                drive.positionSpring = joint.spring_position.y * 1.0f;
                conf.yDrive = drive;
            }
            if (joint.spring_position.z != 0.0f)
            {
                drive = new JointDrive();
                drive.positionSpring = joint.spring_position.z * 1.0f;
                conf.zDrive = drive;
            }

            // Angular
            if (joint.spring_rotation.x != 0.0f)
            {
                drive = new JointDrive();
                drive.mode = JointDriveMode.PositionAndVelocity;
                drive.positionSpring = joint.spring_rotation.x;
                conf.angularXDrive = drive;
            }
            if (joint.spring_rotation.y != 0.0f || joint.spring_rotation.z != 0.0f)
            {
                drive = new JointDrive();
                drive.mode = JointDriveMode.PositionAndVelocity;
                drive.positionSpring = (joint.spring_rotation.y + joint.spring_rotation.z) * 0.5f;
                conf.angularYZDrive = drive;
            }
        }
    }
}
