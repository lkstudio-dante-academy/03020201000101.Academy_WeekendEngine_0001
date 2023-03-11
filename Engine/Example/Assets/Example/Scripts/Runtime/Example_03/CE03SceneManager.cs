using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 스크립트란?
 * - 사용자에 의해서 정의되는 컴포넌트를 의미한다. (즉, 엔진에서 미리 만들어서 제공하는 컴포넌트가 아니라 사용자가 제작하는
 * 프로그램의 목적에 따라 직접 정의해서 사용 할 수 있는 컴포넌트를 의미한다.)
 * 
 * 단, 스크립트가 컴포넌트가 되기 위해서는 2 가지 규칙을 따를 필요가 있다.
 * 
 * 스크립트 컴포넌트 규칙
 * - 스크립트 파일명과 C# 클래스 이름이 서로 동일해야한다.
 * - MonoBehaviour 클래스를 직/간접적으로 상속해야한다.
 */
/** Example 3 */
public class CE03SceneManager : CSceneManager {
	#region 변수
	[SerializeField] private GameObject m_oTarget = null;
	[SerializeField] private List<GameObject> m_oBulletList = new List<GameObject>();
	[SerializeField] private List<GameObject> m_oTargetList = new List<GameObject>();
	#endregion // 변수

	#region 함수
	/** 초기화 */
	public override void Awake() {
		base.Awake();

		for(int i = 0; i < m_oBulletList.Count; ++i) {
			m_oBulletList[i].GetComponent<CTriggerDispatcher>().EnterCallbck = this.HandleOnTriggerEnter;
		}
	}

	/** 상태를 갱신한다 */
	public void Update() {
		/*
		 * 유니티는 특정 물체를 회전하기 위한 오일러 회전 방식과 사원수 회전 방식을 제공한다.
		 * 오일러 회전 방식은 물체를 각 축을 기준으로 회전하는 3 개의 회전 변화를 혼합해서 최종적은 물체의 회전 정도를 계산하는
		 * 방식인 반면, 사원수 회전은 4 차원 벡터 (회전 축 + 회전 각도) 를 기준으로 물체의 회전 정도를 계산하는 방식이다.
		 * 
		 * 단순한 회전은 오일러 회전으로도 쉽게 표현 할 수 있지만 복잡한 회전이 필요 할 경우에는 사원수 회전을 사용하는 것이
		 * 회전을 계산하기 좀 더 수월하다.
		 * 
		 * 또한, 유니티는 오일러 회전으로 입력한 값이라고 하더라도 내부적으로는 사원수 회전으로 처리한다. (즉, 사원수 회전이 
		 * 오일러 회전에 비해서 회전에 대한 연산을 빠르게 처리 할 수 있기 때문에 유니티를 비롯한 여러 엔진들이 내부적으로
		 * 사원수 회전 연산을 수행한다는 것을 알 수 있다.)
		 */
		// 왼쪽 또는 오른쪽 키가 눌렸을 경우
		if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) {
			float fAngle = Input.GetKey(KeyCode.LeftArrow) ? -90.0f : 90.0f;
			m_oTarget.transform.localEulerAngles += new Vector3(0.0f, fAngle, 0.0f) * Time.deltaTime;
		}

		// 위쪽 또는 아래쪽 키가 눌렸을 경우
		if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)) {
			float fPosZ = Input.GetKey(KeyCode.UpArrow) ? 750.0f : -750.0f;
			m_oTarget.transform.localPosition += (m_oTarget.transform.forward * fPosZ) * Time.deltaTime;
		}

		var stDirection = new Vector4(m_oTarget.transform.forward.x, m_oTarget.transform.forward.y, m_oTarget.transform.forward.z, 0.0f);
		var stWorldDirection = m_oTarget.transform.parent.localToWorldMatrix * stDirection;

		/*
		 * Ray 는 반직선을 표현하기 위한 자료형을 의미한다. (즉, 해당 정보를 활용하면 직선을 통한 충돌 등의 연산에 사용하는
		 * 것이 가능하다.)
		 * 
		 * Physics.Raycast 메서드는 광선 추적을 연산하는 역할을 수행한다. (즉, 해당 메서드를 활용하면 3 차원 공간 상에 특정
		 * 방향으로 물체가 존재하는지 여부를 확인하는 것이 가능하다.)
		 */
		var stRay = new Ray(m_oTarget.transform.position, stWorldDirection.normalized);

		// 물체가 존재 할 경우
		if(Physics.Raycast(stRay, out RaycastHit stRaycastHit, 300.0f)) {
			Debug.LogFormat("{0} 이 전방에 등장했습니다!", stRaycastHit.collider.name);
		}
	}

	/*
	 * 유니티 씬 뷰애 특정 정보를 그리기 위한 메서드이다. (즉, 해당 메서드에서 씬 뷰에 프로젝트 개발을 위한 여러 부가 정보를
	 * 표시 할 수 있으며 해당 결과물은 게임 뷰에 전혀 영향을 미치지 않는다는 것을 알 수 있다.)
	 * 
	 * 기즈모란?
	 * - 3 차원 공간 상에 그려지는 2 차원 아이콘 이미지를 의미한다.
	 * 따라서, 프로젝트 진행 위한 여러 부가적인 정보를 아이콘 형태로 의미를 함축 시켜서 표현하는 것이 가능하다.
	 */
	/** 기즈모를 그린다 */
	public void OnDrawGizmos() {
		// 타겟이 존재 할 경우
		if(m_oTarget != null) {
			/*
			 * Gizmos 클래스는 모든 스크립트가 공유하는 클래스이기 때문에 해당 클래스에 특정 정보를 설정하면 모든 스크립트가
			 * 영향을 받는 특징이 존재한다. 따라서, 씬 뷰에 특정 물체를 그리기 위해서 Gizmos 클래스의 정보를 변경했다면 이후
			 * 다시 원래 정보를 되돌려 줄 필요가 있다.
			 */
			Color stPrevColor = Gizmos.color;

			try {
				var stPos = m_oTarget.transform.position;

				/*
				 * Transform 컴포넌트는 내부적으로 설정 된 위치, 회전, 비율 정보를 연산해서 최종적으로 화면 상에 출력하기 위해서
				 * 각각의 정보를 4 x 4 행렬로 변환 후 해당 행렬을 S R T 순서로 곱해서 최종적으로 월드 상에 위치하는 변환 정보를
				 * 연산한다.
				 * 
				 * 따라서, 특정 지역 또는 월드 공간에 존재하는 변환 정보를 특정 지역 공간으로 변환하기 위해서 Transform 컴포넌트는
				 * localToWorldMatrix 또는 worldToLocalMatrix 프로퍼티를 제공한다. (즉, 각각 지역 => 월드, 월드 => 지역 공간으로
				 * 변환 할 수 있는 4 x 4 변환 행렬이라는 것을 알 수 있다.)
				 * 
				 * 또한, 해당 행렬과 연산을 하기 위해서는 4 차원 공간에 해당하는 벡터 정보가 사용되며 이때, 마지막 w 요소는 0 또는
				 * 1 값을 지닌다.
				 * 
				 * 0 은 해당 벡터 정보가 방향 + 크기 정보라는 것을 의미하며, 1 은 해당 벡터 정보가 위치 정보라는 것을 의미한다.
				 * 따라서, 해당 요소가 0 일 경우 변환 행렬에 위치 변환은 전혀 아무런 영향을 미치지 않는다.
				 * (즉, 벡터는 위치에 대한 개념이 존재하지 않기 때문이다.)
				 * 
				 * normalized 프로퍼티란?
				 * - 벡터를 정규화 시킨 정보를 반환해주는 프로퍼티이다. (즉, 해당 프로퍼티를 활용하면 벡터 정보에서 크기 정보는
				 * 제외되고 순수한 방향 정보만 지니고 있는 벡터를 반환한다는 것을 알 수 있다.)
				 */
				var stDirection = new Vector4(m_oTarget.transform.forward.x, m_oTarget.transform.forward.y, m_oTarget.transform.forward.z, 0.0f);
				var stWorldDirection = m_oTarget.transform.parent.localToWorldMatrix * stDirection;

				Gizmos.color = Color.red;
				Gizmos.DrawLine(stPos, stPos + ((Vector3)stWorldDirection.normalized * 300.0f));
			} finally {
				Gizmos.color = stPrevColor;
			}
		}
	}

	/** 충돌 발생을 처리한다 */
	private void HandleOnTriggerEnter(CTriggerDispatcher a_oSender, Collider a_oCollider) {
		GameObject.Destroy(a_oSender.gameObject);
		GameObject.Destroy(a_oCollider.gameObject);
	}
	#endregion // 함수
}