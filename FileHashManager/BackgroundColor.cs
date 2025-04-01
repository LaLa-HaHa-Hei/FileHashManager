using System.Windows.Media;

namespace FileHashManager
{
    /// <summary>
    /// ��������һ�ֲ�ͬ����ɫ
    /// </summary>
    public class BackgroundColor
    {
        private int _index = 0;
        private readonly byte _step = 23;
        private byte _accumulatedStep = 0;

        public Brush GetNextColor()
        {
            // �������ص� 0 ʱ�������ۻ����� (byte ���Զ�����)
            if (_index == 0)
                _accumulatedStep += _step;

            // ��ʼ�� RGB ����Ϊ��ɫ
            byte[] rgb = { 255, 255, 255 };

            // ���ݵ�ǰ����������Ӧ����ɫ���� (byte ���Զ�����)
            rgb[_index] -= _accumulatedStep;

            // ����������ѭ��0~2
            _index = (_index + 1) % 3;

            return new SolidColorBrush(Color.FromRgb(rgb[0], rgb[1], rgb[2]));
        }
    }
}